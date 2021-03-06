﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using HospitalitySkill.Services;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Newtonsoft.Json;

namespace HospitalitySkill.Models.ActionDefinitions
{
    public class LateCheckOutInput : IActionInput
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        public async Task Process(ITurnContext context, IStatePropertyAccessor<HospitalitySkillState> stateAccessor, IStatePropertyAccessor<HospitalityUserSkillState> userStateAccessor, IHotelService hotelService, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(Time))
            {
                var state = await stateAccessor.GetAsync(context, () => new HospitalitySkillState());
                state.LuisResult = new HospitalityLuis
                {
                    Entities = new HospitalityLuis._Entities
                    {
                        datetime = new DateTimeSpec[]
                        {
                            new DateTimeSpec("time", new string[] { Time })
                        }
                    }
                };
            }
        }
    }
}
