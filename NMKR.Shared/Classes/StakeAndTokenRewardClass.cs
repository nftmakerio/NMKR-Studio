namespace NMKR.Shared.Classes
{
    public class StakeAndTokenRewardClass
    {
        private long _stakeReward = 0;
        private long _tokenReward = 0;
        public long StakeReward
        {
            get => _stakeReward;
            set
            {
                _stakeReward = value;
                if (_tokenReward + _stakeReward > 1000000) _tokenReward -= 1000000-_stakeReward;
                if (_tokenReward<0)
                    _tokenReward = 0;
            }
        }

        public long TokenReward
        {
            get => _tokenReward;
            set
            {
                _tokenReward = value;
                if (_tokenReward + _stakeReward > 1000000) _tokenReward -= 1000000 - _stakeReward;
                if (_tokenReward < 0)
                    _tokenReward = 0;
            }
        }

        public long TotalRewards => _tokenReward+ _stakeReward;
    }
}
