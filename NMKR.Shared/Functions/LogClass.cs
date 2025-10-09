using NMKR.Shared.Model;
using System;

namespace NMKR.Shared.Classes
{
    public static class LogClass
    {
        public static void LogMessage(EasynftprojectsContext db, string message, string data = "")
        {
            try
            {
                if (message.Length > 255)
                    message = message.Substring(0, 255);

                db.Backgroundtaskslogs.Add(new() {Created = DateTime.Now, Logmessage = message, Additionaldata = data});
                db.SaveChanges();
            }
            catch {
            }
        }
    }
}
