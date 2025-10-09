using System;
using System.IO;
using NMKR.Shared.Classes;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Cli
{
    public static class CliFunctions
    {
        public static Querytip GetQueryTipFromCli(bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string t = GlobalFunctions.GetGuid();
            string command =
                $"query tip --out-file {GeneralConfigurationClass.TempFilePath}{t}.json {(mainnet ? " --mainnet" : $" --testnet-magic {GeneralConfigurationClass.TestnetMagicId}")}";

            buildtransaction.LogFile += command + Environment.NewLine;
            var s = ConsoleCommand.CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += s + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!File.Exists($"{GeneralConfigurationClass.TempFilePath}{t}.json"))
            {
                return null;
            }

            Querytip qt = JsonConvert.DeserializeObject<Querytip>(File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}.json"));
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.json");
            return qt;
        }

      
    }
}
