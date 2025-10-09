namespace NMKR.Shared.Blockchains
{
    public class CreateCollectionParameterClass
    {
        public CreateCollectionParameterClass(string description, string image, string mimetype, string name, int sellerFeeBasisPoints, string symbol, string externalurl)
        {
            Description = description;
            Image = image;
            Mimetype = mimetype;
            Name = name;
            SellerFeeBasisPoints = sellerFeeBasisPoints;
            Symbol = symbol;
            Externalurl = externalurl;
        }

        public string Description { get; private set; }
        public string Image { get; private set; }
        public string Mimetype { get; private set; }
        public string Name { get; private set; }
        public int SellerFeeBasisPoints { get; private set; }
        public string Symbol { get; private set; }
        public string Externalurl { get; private set; }
    }
}
