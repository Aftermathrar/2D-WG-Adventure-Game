using System.Collections.Generic;

namespace ButtonGame.Inventories
{
    public interface ITooltipProvider
    {
        string GetDisplayName();
        string GetCategoryName();
        IEnumerable<TooltipDescriptionField> GetDescriptionFields();
    }
}
