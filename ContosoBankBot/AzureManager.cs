using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using ContosoBankBot.DataModels;
using System.Threading.Tasks;

namespace ContosoBankBot
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;

        private IMobileServiceTable<Account> accTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contosodummyapp.azurewebsites.net");
            this.accTable = this.client.GetTable<Account>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task UpdateAccount(Account acct)
        {
            await this.accTable.UpdateAsync(acct);
        }

        public async Task<List<Account>> GetPassword(string id)
        {
            return await this.accTable.Where(x => x.Password == id).ToListAsync();
        }


        public async Task DeleteAccount(Account acct)
        {
            await this.accTable.DeleteAsync(acct);
        }


        public async Task AddAccount(Account acct)
        {
            await this.accTable.InsertAsync(acct);
        }

        public async Task<List<Account>> GetAccountTable()
        {
            return await this.accTable.ToListAsync();
        }
    }

}