﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

namespace log4net.MicrosoftTeams
{
    class MicrosoftTeamsClient
    {
        private readonly Uri _uri;
        private static readonly HttpClient Client = new HttpClient();

        public MicrosoftTeamsClient(string url)
        {
            _uri = new Uri(url);
        }

        public void PostMessageAsync(string formattedMessage, List<MicrosoftTeamsMessageFact> facts)
        {
            var message = CreateMessageCard(formattedMessage, facts);
            var json = JsonConvert.SerializeObject(message);

            var result = Client.PostAsync(_uri, new StringContent(json, Encoding.UTF8, "application/json")).Result;
        }

        private MicrosoftTeamsMessageCard CreateMessageCard(string text, List<MicrosoftTeamsMessageFact> facts)
        {
            var request = new MicrosoftTeamsMessageCard
            {
                Title = text,
                Text = text,
                Color = GetAttachmentColor(facts),
                Sections = new[]
                {
                    new MicrosoftTeamsMessageSection
                    {
                        Title = "Properties",
                        Facts = facts.Where(x => !x.Name.StartsWith("Exception")).ToArray()
                    },
                    new MicrosoftTeamsMessageSection
                    {
                        Title = "Exception",
                        Facts = facts.Where(x => x.Name.StartsWith("Exception")).ToArray()
                    }
                }
            };

            return request;
        }

        private static string GetAttachmentColor(List<MicrosoftTeamsMessageFact> facts)
        {
            var level = facts.FirstOrDefault(x => x.Name.StartsWith("Level"));

            return GetAttachmentColor(level?.Value);
        }

        private static string GetAttachmentColor(string level)
        {
            switch (level)
            {
                case "INFO":
                    return "5bc0de";

                case "WARN":
                    return "f0ad4e";

                case "ERROR":
                case "FATAL":
                    return "d9534f";

                default:
                    return "777777";
            }
        }
    }
}
