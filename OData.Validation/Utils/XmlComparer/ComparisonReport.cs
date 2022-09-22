using GillSoft.XmlComparer;

namespace OData.Schema.Validation.Utils.XmlComparer
{
    public class ComparisonReport
    {
        public List<ElementAddedEventArgs> Additions;
        public List<ElementChangedEventArgs> Changes;
        public List<ElementRemovedEventArgs> Removals;

        public ComparisonReport()
        {
            Additions = new List<ElementAddedEventArgs>();
            Changes = new List<ElementChangedEventArgs>();
            Removals = new List<ElementRemovedEventArgs>();

        }

        public ComparisonReport(List<ElementAddedEventArgs> additions, List<ElementChangedEventArgs> changes, List<ElementRemovedEventArgs> removals)
        {
            Additions = additions;
            Changes = changes;
            Removals = removals;
        }
    }
}
