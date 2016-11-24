using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using ContosoBankBot.DataModels;
using ContosoBankBot.Models;
using System.Text;

namespace ContosoBankBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            bool hasAskedName = false;
            string userNameTemp = "";

            if (activity.Type == ActivityTypes.Message)
            {
                StateClient sc = activity.GetStateClient();
                BotData userData = sc.BotState.GetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                hasAskedName = userData.GetProperty<bool>("AskedUserName");
                userNameTemp = userData.GetProperty<string>("user_name") ?? "";

                StringBuilder strReply = new StringBuilder();

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                string userInput = activity.Text;


                // fix here later so that it only responds to hello or hi :TODO
                //if ((userInput.ToLower().ToString().Equals("hello")) || (userInput.ToLower().ToString().Equals("hi")))
                //{


                if (hasAskedName == false)
                {
                    strReply.Append($"Hello! I am contoso bot. What is your name?");

                    userData.SetProperty<bool>("AskedUserName", true);   // set it to true since name was asked
                }
                else if (userNameTemp == "")
                {
                    strReply.Append($"Hello {activity.Text}!, Please enter contoso to continue our service :)");
                    userData.SetProperty<string>("user_name", activity.Text);
                }

                else if(userNameTemp.Equals(userData.GetProperty<string>("user_name")) && (userInput.ToLower().ToString().Contains("hello") || userInput.ToLower().ToString().Contains("hi")))
                {
                    strReply.Append("Hello again! Please enter contoso for more info ! Thanks :)");
                }

                //}

                if (userInput.ToLower().Equals("contoso"))
                {
                    Activity replyToConversation = activity.CreateReply("Visit Contoso Bank");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardImage> cardImg = new List<CardImage>();
                    cardImg.Add(new CardImage(url: "https://s15.postimg.org/amhmq1xcr/contosologo.png"));

                    List<CardAction> cardBut = new List<CardAction>();
                    CardAction plBut = new CardAction()
                    {
                        Value = "http://msa.ms",
                        Type = "openUrl",
                        Title = "Contoso Website"
                    };

                    CardAction plBut1 = new CardAction()
                    {
                        Value = "show account",
                        Type = "imBack",
                        Title = "Show Account"
                    };


                    CardAction plBut2 = new CardAction()
                    {
                        Value = "tutorial",
                        Type = "imBack",
                        Title = "Tutorial"
                    };

                    CardAction plBut3 = new CardAction()
                    {
                        Value = "conversion help",
                        Type = "imBack",
                        Title = "Currency Conversion"
                    };


                    cardBut.Add(plBut);
                    cardBut.Add(plBut1);
                    cardBut.Add(plBut2);
                    cardBut.Add(plBut3);

                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Simpler, Faster Banking Solution",
                        Subtitle = "What would you like to do?",
                        Images = cardImg,
                        Buttons = cardBut

                    };

                    Attachment plAttach = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttach);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                else if (userInput.ToLower().ToString().Equals("tutorial"))
                {
                    strReply.Append("This is a tutorial how to use contoso bot, Let's begin\n\n");
                    strReply.Append("To log in:\n\n show account (account_number) (password)\n\n");
                    strReply.Append("To create account:\n\n create account (account number) (account_name) (desired password)\n\n");
                    strReply.Append("To update password:\n\n update password (account_number) (password) (new password)\n\n");
                    strReply.Append("To delete an account:\n\n delete account (account_number) (password)\n\n");
                    strReply.Append("Please disregard brackets when you type. Thank you :)");

                }

                // show account to the user to choose from
                else if (userInput.ToLower().Equals("show account"))
                {
                    List<Account> acc = await AzureManager.AzureManagerInstance.GetAccountTable();

                    strReply.Append("Current accounts\n\n");

                    foreach (Account a in acc)
                    {
                        strReply.Append($"Account Number: {a.AccNum.ToString()}\n\n");

                    }

                    //strReply.Append("Please type \"show account ACC_NUMBER ACC_PASSWORD\" to log into your account");
                }

                else if ((userInput.ToLower().Contains("update account")) && (userInput.Length > 15))
                {
                    string line = userInput.ToLower();
                    string[] strSplit = line.Split(' ');
                    bool isCorrect = false;

                    if (strSplit.Length == 5)
                    {
                        //List<Account> acc = await AzureManager.AzureManagerInstance.GetAccountTable();

                        //omg i bloody did it !!

                        var x = await AzureManager.AzureManagerInstance.GetPassword(strSplit[3]);
                        foreach (var a in x)
                        {
                            a.Password = strSplit[4].ToString();
                            await AzureManager.AzureManagerInstance.UpdateAccount(a);
                            isCorrect = true;
                        }

                        //foreach(Account a in acc)
                        //{
                        //    if ((a.AccNum.Equals(strSplit[2].ToString())) && (a.Password.Equals(strSplit[3])))
                        //    {
                        //        string newPW = strSplit[4].ToString();
                        //        a.Password = newPW;
                        //        await AzureManager.AzureManagerInstance.UpdateAccount(a);
                        //        strReply.Append("Password has been updated.");
                        //        isCorrect = true;
                        //        break;

                        //    }
                        //}
                        if (isCorrect == false)
                        {
                            strReply.Append("Please provide correct information. Thanks");
                        }
                    }
                    else
                    {
                        strReply.Append("Please provide correct information. Thanks");
                    }
                }




                // to delete account
                else if (userInput.ToLower().Contains("delete account"))
                {
                    string line = userInput.ToLower();
                    string[] strSplit = line.Split(' ');
                    bool isCorrect = false;

                    if (strSplit.Length == 4)
                    {
                        //int accToDelete = strSplit[2]
                        //string pwToDelete = strSplit[3].ToString();
                        List<Account> acc = await AzureManager.AzureManagerInstance.GetAccountTable();

                        foreach (Account a in acc)
                        {
                            if ((a.AccNum.Equals(strSplit[2].ToString())) && (a.Password.Equals(strSplit[3])))
                            {

                                await AzureManager.AzureManagerInstance.DeleteAccount(a);
                                strReply.Append("Deleted successfully. Thank you.");
                                //return Request.CreateResponse(HttpStatusCode.OK);
                                isCorrect = true;
                                break;
                            }
                        }
                        if (isCorrect == false)
                        {
                            strReply.Append("Please provide correct information. Thank you");
                        }
                    }
                    else
                    {
                        strReply.Append("Please provide correct information. Thanks");
                    }
                }


                // to create new account 
                else if (userInput.ToLower().Contains("create account"))
                {

                    string line = userInput.ToLower();
                    string[] strSplit = line.Split(' ');
                    if (strSplit.Length == 5)
                    {

                        //int newAccNum = Int32.Parse(strSplit[2]);

                        Account newAcc = new Account()
                        {
                            AccNum = strSplit[2].ToString(),
                            AccName = strSplit[3].ToString(),
                            Password = strSplit[4].ToString()

                        };

                        await AzureManager.AzureManagerInstance.AddAccount(newAcc);

                        strReply.Append("New account has been added to the database.");

                    }
                    else
                    {
                        strReply.Append("Please provide correct formant. Thank you");
                    }

                }


                // Verify user's password in order to show user's bank account status
                else if ((userInput.ToLower().Contains("show account")) && (userInput.Length > 13))
                {
                    string line = userInput.ToLower();
                    string[] strSplit = line.Split(' ');
                    bool isCorrect = false;

                    if (strSplit.Length == 4)
                    {
                        List<Account> acc = await AzureManager.AzureManagerInstance.GetAccountTable();

                        foreach (Account a in acc)
                        {
                            if (a.AccNum.Equals(strSplit[2].ToString()) && (a.Password.Equals(strSplit[3].ToString())))
                            {
                                // break: to stop the loop for repeated messages


                                strReply.Append($"Login successful!\n\n");
                                strReply.Append($"Account number [{a.AccNum.ToString()}] Details\n\n");
                                strReply.Append($"Balance: [$ {a.Balance.ToString()}]\n\n");
                                strReply.Append($"Savings: [$ {a.SavingAcc.ToString()}]\n\n");
                                strReply.Append($"Interest Rate: [% {a.IntRate.ToString()}]\n\n");
                                isCorrect = true;
                                break;
                            }

                        }

                        if (isCorrect == false)
                        {
                            strReply.Append("Please provide correct information.");
                        }
                    }
                    else
                    {
                        strReply.Append("Please provide correct format. Thank you");
                        //return Request.CreateResponse(HttpStatusCode.OK);
                    }


                }

                else if (userInput.ToLower().ToString().Equals("conversion help"))
                {
                    strReply.Append("In order to use currency conversion service, Please follow the following format\n\n");
                    strReply.Append("convert NZD(from) 10000(money you have) to AUD(desired currency)\n\n");
                    strReply.Append("Please ignore ( ) when you actually type. Thank you!");
                }


                // currency conversion API 
                else if (userInput.ToLower().Contains("convert"))
                {
                    string line = userInput.ToString();
                    string[] data = line.Split(' ');

                    if (data.Length == 5)
                    {


                        decimal fromRate;
                        decimal toRate;

                        decimal moneyToConvert = Decimal.Parse(data[2].ToString());

                        //testing
                        //strReply.Append(moneyToConvert.ToString());
                        //strReply.Append($"THe fuck {data[0]},{data[1]},{data[2]},{data[3]},{data[4]}");


                        string curr_from = data[1].ToUpper();
                        string curr_to = data[4].ToUpper();

                        var url = "https://openexchangerates.org/api/latest.json?app_id=d8c2e65337e648dfb4469a9a7068aad5";
                        var currencyRates = download_serialized_json_data<CurrencyRates>(url);

                        if (currencyRates.Rates.TryGetValue(curr_from, out fromRate))
                        {
                            moneyToConvert /= fromRate;
                        }

                        if (currencyRates.Rates.TryGetValue(curr_to, out toRate))
                        {
                            moneyToConvert *= toRate;
                        }

                        //moneyToConvert.ToString("#.##");

                        //decimal.Round(moneyToConvert, 2);

                        strReply.Append(currencyRates.Disclaimer);
                        strReply.Append(currencyRates.License);
                        strReply.Append("\n\n--------------------------------\n\n\n");
                        strReply.Append($"Your converted money is [{decimal.Round(moneyToConvert, 2)}] in [{curr_to.ToUpper()}]");

                    }
                    else
                    {
                        strReply.Append("Please provide correct information");
                    }



                }

                sc.BotState.SetPrivateConversationData(activity.ChannelId, activity.Conversation.Id, activity.From.Id, userData);

                Activity replymsg = activity.CreateReply(strReply.ToString());
                await connector.Conversations.ReplyToActivityAsync(replymsg);

            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private static T download_serialized_json_data<T>(string url) where T : new()
        {
            using (var c = new WebClient())
            {
                var json_data = string.Empty;
                try
                {
                    json_data = c.DownloadString(url);
                }
                catch (Exception) { }

                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T(); 
            }
        }




        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}