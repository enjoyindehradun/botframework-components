﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using ITSMSkill.Dialogs.Teams;
using ITSMSkill.Models;
using ITSMSkill.Models.Actions;
using ITSMSkill.Prompts;
using ITSMSkill.Responses.Knowledge;
using ITSMSkill.Responses.Shared;
using ITSMSkill.Responses.Ticket;
using ITSMSkill.Services;
using ITSMSkill.Utilities;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Responses;

namespace ITSMSkill.Dialogs
{
    public class CreateTicketDialog : SkillDialogBase
    {
        public CreateTicketDialog(
             IServiceProvider serviceProvider)
            : base(nameof(CreateTicketDialog), serviceProvider)
        {
            var createTicket = new WaterfallStep[]
            {
                CheckTitleAsync,
                InputTitleAsync,
                SetTitleAsync,
                DisplayExistingLoopAsync,
                CheckDescriptionAsync,
                InputDescriptionAsync,
                SetDescriptionAsync,
                CheckUrgencyAsync,
                InputUrgencyAsync,
                SetUrgencyAsync,
                GetAuthTokenAsync,
                AfterGetAuthTokenAsync,
                CreateTicketAsync
            };

            // TaskModule Based WaterFallStep
            var createTicketTaskModule = new WaterfallStep[]
            {
                GetAuthTokenAsync,
                AfterGetAuthTokenAsync,
                CreateTicketTeamsTaskModuleAsync
            };

            var displayExisting = new WaterfallStep[]
            {
                GetAuthTokenAsync,
                AfterGetAuthTokenAsync,
                ShowKnowledgeAsync,
                IfKnowledgeHelpAsync
            };

            AddDialog(new WaterfallDialog(Actions.CreateTicket, createTicket));
            AddDialog(new WaterfallDialog(Actions.DisplayExisting, displayExisting));
            AddDialog(new WaterfallDialog(Actions.CreateTicketTeamsTaskModule, createTicketTaskModule));

            InitialDialogId = Actions.CreateTicket;

            // intended null
            // ShowKnowledgeNoResponse
            ShowKnowledgeHasResponse = KnowledgeResponses.ShowExistingToSolve;
            ShowKnowledgeEndResponse = KnowledgeResponses.KnowledgeEnd;
            ShowKnowledgeResponse = KnowledgeResponses.IfExistingSolve;
            ShowKnowledgePrompt = Actions.NavigateYesNoPrompt;
            KnowledgeHelpLoop = Actions.DisplayExisting;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext dc, object options, CancellationToken cancellationToken = default)
        {
            if (dc.Context.Activity.ChannelId.Contains(Channels.Msteams))
            {
                return await dc.BeginDialogAsync(Actions.CreateTicketTeamsTaskModule, options, cancellationToken);
            }

            return await base.OnBeginDialogAsync(dc, options, cancellationToken);
        }

        protected async Task<DialogTurnResult> CreateTicketTeamsTaskModuleAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var reply = sc.Context.Activity.CreateReply();

            var adaptiveCard = TicketDialogHelper.ServiceNowTickHubAdaptiveCard();
            reply = sc.Context.Activity.CreateReply();
            reply.Attachments = new List<Attachment>()
            {
                new Microsoft.Bot.Schema.Attachment() { ContentType = AdaptiveCard.ContentType, Content = adaptiveCard }
            };

            await sc.Context.SendActivityAsync(reply, cancellationToken);
            return await sc.EndDialogAsync();
        }

        protected async Task<DialogTurnResult> DisplayExistingLoopAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);

            if (state.DisplayExisting)
            {
                state.PageIndex = -1;
                return await sc.BeginDialogAsync(Actions.DisplayExisting, cancellationToken: cancellationToken);
            }
            else
            {
                return await sc.NextAsync(cancellationToken: cancellationToken);
            }
        }

        protected async Task<DialogTurnResult> CreateTicketAsync(WaterfallStepContext sc, CancellationToken cancellationToken = default(CancellationToken))
        {
            var state = await StateAccessor.GetAsync(sc.Context, () => new SkillState(), cancellationToken);
            var management = ServiceManager.CreateManagement(Settings, sc.Result as TokenResponse, state.ServiceCache);
            var result = await management.CreateTicket(state.TicketTitle, state.TicketDescription, state.UrgencyLevel);

            if (!result.Success)
            {
                return await SendServiceErrorAndCancelAsync(sc, result, cancellationToken);
            }

            var card = GetTicketCard(sc.Context, state, result.Tickets[0]);

            await sc.Context.SendActivityAsync(TemplateManager.GenerateActivity(TicketResponses.TicketCreated, card, null), cancellationToken);
            return await sc.EndDialogAsync(await CreateActionResultAsync(sc.Context, true, cancellationToken), cancellationToken);
        }
    }
}
