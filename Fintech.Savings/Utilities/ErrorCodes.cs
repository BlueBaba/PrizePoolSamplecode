namespace App.Utilities
{
    public static class ErrorCodes
    {
        internal static readonly string Contributors_And_NoOInstallment_Mismatch = "101";
        internal static readonly string Successful = "00";
        internal static readonly string Administrator_Not_A_contributor = "102";
        internal static readonly string No_Contributors_Found = "103";
        internal static readonly string Savings_Code_Not_Found = "104";
        internal static readonly string Request_Already_Accepted = "105";
        internal static readonly string Savings_Plan_Not_Valid;
        internal static readonly string Withdrawal_Threshold_Reached;
        internal static readonly string Savings_Plan_Already_Exist = "110"; 
        internal static readonly string Invalid_Start_Date = "107";

        internal static readonly string InNon_Distinct_Contributors_PaymentInstrumentReference = "109";

        public static string Customer_Not_In_Contributors_List = "106";
        public static string Request_Successful = "00";
        public static string Request_Not_Successful = "01";

        internal static readonly string Savings_Exist_For_Same_Administrator = "108";

        public const string Processing = "55";
        public const string RequestFailed = "97";
        public const string TransactionFailed = "98";
        public const string ServerError = "99";
        public const string ServiceUnavailable = "100";

        public const string Invalid_Credential = "20";
    }

   

}
