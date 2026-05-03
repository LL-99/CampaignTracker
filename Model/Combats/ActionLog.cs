namespace CampaignTracker.Model.Combats
{
    public class ActionLog
    {
        public List<ActionLogEntry> Entries { get; private set; } = [];

        public ActionLogEntry AddEntry(Guid combat, IEnumerable<Guid> actors, params ActionEffect[] effects)
        {
            var entry = new ActionLogEntry
            {
                Combat = combat,
                Actors = actors.ToArray(),
                Effects = effects
            };

            Entries.Add(entry);

            return entry;
        }
    }
}
