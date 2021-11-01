using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace App.Entities
{


    public enum FrontendChannelsEnum
    {
        Ussd = 1,
        Mobile = 2,
        Web = 3,

    }



    public enum InterestRateCalulation
    {
        [Description("PD")]
        PerDay = 1,
        [Description("PM")]
        PerMonth = 2,
        [Description("PA")]
        PerAnnum = 3
    }


}
