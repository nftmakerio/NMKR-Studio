namespace NMKR.Shared.Enums
{
    public enum TransactionTypes
    {
        paidonftaddress,
        mintfromcustomeraddress,
        paidtocustomeraddress,
        paidfromnftaddress,
        consolitecustomeraddress,
        paidfailedtransactiontocustomeraddress,
        doublepaymentsendbacktobuyer,
        paidonprojectaddress,
        fiatpayment,
        mintfromnftmakeraddress,
        burning,
        decentralmintandsend,
        decentralmintandsale,
        royaltsplit,
        unknown,
        directsale,
        auction,
        buymints,
        refundmints
    }


    public enum ProceedPaymentTransactionCommands
    {
        GetTransactionState,
        GetPaymentAddress,
        SignDecentralPayment,
        CheckPaymentAddress,
        CancelTransaction,
        EndTransaction,
        GetPriceListForProject,
        LockNft,
        SubmitTransaction,
        BetOnAuction,
        BuyDirectSale,
        ReservePaymentgatewayMintAndSendNft,
        MintAndSendPaymentgatewayNft,
        UpdateCustomProperties,
        LockAda,
        SellDirectSaleOffer,
        GetBuyoutSmartcontractAddress,
        ExtendReservationTime
    }

    public enum DatumTemplateTypes
    {
        locknft,
        bet,
        buy,
        close,
        cancel,
        lockada,
        sell
    }

}
