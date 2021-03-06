﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace EmailSkill.Models.Action
{
    public class ActionResult
    {
        public ActionResult(bool actionSuccess)
        {
            ActionSuccess = actionSuccess;
        }

        [JsonProperty("actionSuccess")]
        public bool ActionSuccess { get; set; }
    }
}
