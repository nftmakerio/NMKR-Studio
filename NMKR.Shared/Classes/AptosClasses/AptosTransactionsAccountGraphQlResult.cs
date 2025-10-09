using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.AptosClasses
{
    public partial class AptosTransactionsAccountGraphQlResult
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("account_transactions", NullValueHandling = NullValueHandling.Ignore)]
        public AccountTransaction[] AccountTransactions { get; set; }
    }

    public partial class AccountTransaction
    {
        [JsonProperty("transaction_version", NullValueHandling = NullValueHandling.Ignore)]
        public long? TransactionVersion { get; set; }

        [JsonProperty("user_transaction", NullValueHandling = NullValueHandling.Ignore)]
        public UserTransaction UserTransaction { get; set; }

        [JsonProperty("coin_activities", NullValueHandling = NullValueHandling.Ignore)]
        public CoinActivity[] CoinActivities { get; set; }
    }

    public partial class CoinActivity
    {
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("coin_type", NullValueHandling = NullValueHandling.Ignore)]
        public string CoinType { get; set; }

        [JsonProperty("activity_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ActivityType { get; set; }

        [JsonProperty("is_gas_fee", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGasFee { get; set; }

        [JsonProperty("is_transaction_success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsTransactionSuccess { get; set; }
    }

    public partial class UserTransaction
    {
        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public long? Version { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("sender", NullValueHandling = NullValueHandling.Ignore)]
        public string Sender { get; set; }

        [JsonProperty("entry_function_function_name", NullValueHandling = NullValueHandling.Ignore)]
        public string EntryFunctionFunctionName { get; set; }

        [JsonProperty("entry_function_id_str", NullValueHandling = NullValueHandling.Ignore)]
        public string EntryFunctionIdStr { get; set; }

        [JsonProperty("entry_function_module_name", NullValueHandling = NullValueHandling.Ignore)]
        public string EntryFunctionModuleName { get; set; }

        [JsonProperty("sequence_number", NullValueHandling = NullValueHandling.Ignore)]
        public long? SequenceNumber { get; set; }
    }
}
