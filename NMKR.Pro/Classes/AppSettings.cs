using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes.CustomerData;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using NMKR.RazorSharedClassLibrary.Components;

namespace NMKR.Pro.Classes
{
    public class AppSettings
    {
        private int? _userid { get; set; }

        public event Action OnChange;
        public int? UserId
        {
            get { return _userid; }
            set
            {
                _userid = value;

                if (_userid == null)
                {
                    hash = null;
                    return;
                }

                LockedCustomerAssetsCheck = new LockedCustomerAssetsCheck( _userid ?? 0);
            }
        }
        private readonly EasynftprojectsContext _db = new(GlobalFunctions.optionsBuilder.Options);

        public int? PendingUserId { get; set; }
        public LockedCustomerAssetsCheck LockedCustomerAssetsCheck { get; set; }
        private void NotifyStateChanged() => OnChange?.Invoke();

        public Customer customer;
        public string CustomerName { get; set; }
        public string Avatar { get; set; }
        public string hash { get; set; }

        private bool isExtending = false;

        private bool _drawerOpen = true;
        private bool _showKycAlert;
        private string _kycState;
        private DateTime validUntil;

        public PreserveTableStateClass ProjectsTable = new() { EntriesPerPage = 10, PageNumber = 0, Parameter1 = "Active", Search = " ", SortOrder = "id" };
        
        public bool DrawerOpen
        {
            get
            {
                return _drawerOpen;
            }
            set
            {
                _drawerOpen = value;
                NotifyStateChanged();
            }
        }

        public bool InformationWindowShowed { get; set; }

      
        public void DrawerToggle()
        {
            DrawerOpen = !DrawerOpen;
        }
        public void Logout()
        {
            UserId = null;
            hash = null;
            PendingUserId = null;
            CustomerName = "";
            Avatar = "";
        }

        public async Task ExtendLoginTime()
        {
            if (isExtending)
                return;

            isExtending = true;
            if (UserId != null && !string.IsNullOrEmpty(hash))
            {
                var h = await (from a in _db.Loggedinhashes
                    where a.Hash == hash && a.CustomerId == UserId
                    select a).FirstOrDefaultAsync();
                if (h == null)
                {
                    Logout();
                    isExtending = false;
                    return;
                }

                h.Lastlifesign = DateTime.Now;
                h.Validuntil = h.Validuntil.AddMinutes(60);
                validUntil=h.Validuntil;
                await _db.SaveChangesAsync();
            }

            isExtending = false;
        }

        public string KycState
        {
            get => _kycState;
            set
            {
                _kycState = value;
                NotifyStateChanged();
            }
        }

        public bool ShowKycAlert
        {
            get => _showKycAlert;
            set
            {
                _showKycAlert = value;
                NotifyStateChanged();
            }
        }

        public string GetRemainingTime()
        {
            var dateOne = DateTime.Now;
            var dateTwo = validUntil;
            var diff = dateOne.Subtract(dateTwo);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            return res;
        }
    }
}
