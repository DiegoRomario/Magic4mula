
using System.ComponentModel;

namespace M4.WebApi.Models
{
    public enum ECriterio
    {
        [Description("PL + ROE")]
        PL_ROE = 0,
        [Description("EV/EBIT + ROIC")]
        EVEBIT_ROIC = 1
    }

}
