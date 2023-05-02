#if !Debug
using System;
#endif
using System.Collections.Generic;
#if Debug
using System.Diagnostics;
#endif
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace FallGuysStats {
    internal class FallalyticsReporter {
        private List<RoundInfo> roundList = new List<RoundInfo>();

        private static readonly string APIEndpoint = "https://fallalytics.com/api/report";

        private static readonly HttpClient HttpClient = new HttpClient();

        public async void Report(RoundInfo stat, string APIKey) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, APIEndpoint);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", APIKey);

            request.Content = new StringContent(this.RoundInfoToJSONString(stat), Encoding.UTF8, "application/json");
            try {
                await HttpClient.SendAsync(request);
            } catch (HttpRequestException e) {
#if Debug
                Debug.WriteLine($@"Error in FallalyticsReporter. Should not be a problem as it only affects the reporting. Error: {e.Message}");
#else
                Console.WriteLine($@"Error in FallalyticsReporter. Should not be a problem as it only affects the reporting. Error: {e.Message}");
#endif
            }

            if (stat.Round == 1) {
                foreach (RoundInfo roundInfo in this.roundList.ToList()) {
                    if (roundInfo.ShowID != stat.ShowID) {
                        this.roundList.Remove(roundInfo);
                    }
                }
            }

            this.roundList.Add(stat);

            bool foundMissMatch = false;
            int finalRound = -1;
            foreach (RoundInfo roundInfo in this.roundList.ToList()) {
                if (roundInfo.SessionId != stat.SessionId) {
                    foundMissMatch = true;
                }
                if (roundInfo.IsFinal) {
                    finalRound = roundInfo.Round;
                }
            }
            if (this.roundList.Count == finalRound && !foundMissMatch) {
                this.ShowComplete(APIKey);
            }
        }
        public async void ShowComplete(string APIKey) {
            HttpRequestMessage requestArray = new HttpRequestMessage(HttpMethod.Post, APIEndpoint);

            requestArray.Headers.Authorization = new AuthenticationHeaderValue("Bearer", APIKey);

            string jsonArraystring = "[";
            foreach (RoundInfo game in this.roundList) {
                jsonArraystring += this.RoundInfoToJSONString(game);
                jsonArraystring += ",";
            }
            jsonArraystring = jsonArraystring.Remove(jsonArraystring.Length - 1);
            jsonArraystring += "]";

            requestArray.Content = new StringContent(jsonArraystring, Encoding.UTF8, "application/json");

            try {
                await HttpClient.SendAsync(requestArray);
            } catch (HttpRequestException e) {
#if Debug
                Debug.WriteLine($@"Error in FallalyticsReporter. Should not be a problem as it only affects the reporting. Error: {e.Message}");
#else
                Console.WriteLine($@"Error in FallalyticsReporter. Should not be a problem as it only affects the reporting. Error: {e.Message}");
#endif
            }

            this.roundList = new List<RoundInfo>();
        }
        public string RoundInfoToJSONString(RoundInfo round) {
            string json = "";
            json += "{\"round\":\"" + round.Name + "\",";
            json += "\"index\":" + round.Round + ",";
            json += "\"show\":\"" + round.ShowNameId + "\",";
            json += "\"isfinal\":" + (round.IsFinal ? "true" : "false") + ",";
            json += "\"session\":\"" + round.SessionId + "\"}";
            return json;
        }
    }
}