﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using EmailSkill.Extensions;
using EmailSkill.Responses.Shared;
using Microsoft.Bot.Solutions.Resources;
using Microsoft.Graph;

namespace EmailSkill.Utilities
{
    public class SpeakHelper
    {
        public static string ToSpeechEmailListString(List<Message> messages, TimeZoneInfo timeZone, int maxReadSize = 1)
        {
            var speakString = string.Empty;

            if (messages == null || messages.Count == 0)
            {
                return speakString;
            }

            var emailDetail = string.Empty;

            var readSize = Math.Min(messages.Count, maxReadSize);
            if (readSize >= 1)
            {
                emailDetail = ToSpeechEmailDetailOverallString(messages[0], timeZone);
            }

            speakString = emailDetail;
            return speakString;
        }

        public static string ToSpeechEmailDetailOverallString(Message message, TimeZoneInfo timeZone)
        {
            var speakString = string.Empty;

            if (message != null)
            {
                var time = message.ReceivedDateTime == null
                    ? CommonStrings.NotAvailable
                    : message.ReceivedDateTime.Value.UtcDateTime.ToRelativeString(timeZone);
                var sender = (message.Sender?.EmailAddress?.Name != null) ? message.Sender.EmailAddress.Name : EmailCommonStrings.UnknownSender;
                var subject = message.Subject ?? EmailCommonStrings.EmptySubject;
                speakString = string.Format(EmailCommonStrings.FromDetailsFormatAll, sender, time, subject);
            }

            return speakString;
        }

        public static string ToSpeechEmailDetailString(Message message, TimeZoneInfo timeZone, bool isNeedContent = false)
        {
            var speakString = string.Empty;

            if (message != null)
            {
                var time = message.ReceivedDateTime == null
                    ? CommonStrings.NotAvailable
                    : message.ReceivedDateTime.Value.UtcDateTime.ToRelativeString(timeZone);
                var subject = message.Subject ?? EmailCommonStrings.EmptySubject;
                var sender = (message.Sender?.EmailAddress?.Name != null) ? message.Sender.EmailAddress.Name : EmailCommonStrings.UnknownSender;
                var content = message.Body.Content ?? EmailCommonStrings.EmptyContent;

                var stringFormat = isNeedContent ? EmailCommonStrings.FromDetailsWithContentFormat : EmailCommonStrings.FromDetailsFormatAll;
                speakString = string.Format(stringFormat, sender, time, subject, content);
            }

            return speakString;
        }

        public static string ToSpeechEmailSendDetailString(string detailSubject, string detailToRecipient, string detailContent)
        {
            var speakString = string.Empty;

            var subject = (detailSubject != string.Empty) ? detailSubject : EmailCommonStrings.EmptySubject;
            var toRecipient = (detailToRecipient != string.Empty) ? detailToRecipient : EmailCommonStrings.UnknownRecipient;
            var content = (detailContent != string.Empty) ? detailContent : EmailCommonStrings.EmptyContent;

            speakString = string.Format(EmailCommonStrings.ToDetailsFormat, subject, toRecipient, content);

            return speakString;
        }
    }
}
