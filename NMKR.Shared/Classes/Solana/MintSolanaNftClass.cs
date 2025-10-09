using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Solnet.Wallet;

namespace NMKR.Shared.Classes.Solana
{
    public class MintSolanaNftClass
    {
        public MintSolanaNftClass(Nft nft, ulong count, string receiverAddress, Wallet projecWallet)
        {
            Nft = nft;
            Count = count;
            ReceiverPublickKey = new PublicKey(receiverAddress);
            ProjectWallet= projecWallet;
        }

        public Wallet ProjectWallet { get; }

        public Account NftAccount { get; set; }

      public PublicKey ReceiverPublickKey { get; }
      public ulong Count { get;  }
      public Solnet.Metaplex.NFT.Library.Metadata Metadata { get; set; }

      public PublicKey TokenSource { get; set; }
      public PublicKey TokenDestination { get; set; }
      public Account ProjectAccount { get; set; }
      public PublicKey MasterEditionKey { get; set; }
      public Nft Nft { get;  }
      public string Network { get;  } = GlobalFunctions.IsMainnet() ? "mainnet" : "devnet";
    }

}
