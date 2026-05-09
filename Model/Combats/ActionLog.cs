namespace CampaignTracker.Model.Combats
{
    public class ActionLog
    {
        public List<ActionLogEntry> Entries { get; private set; } = [];
        public int CurrentTurn { get; set; } = 1;
        public bool IsEndTurnPending => Entries.Count > 0 && CurrentTurn > GetLastTurn();

        public ActionLogEntry AddEntry(Guid combat, IEnumerable<Guid> actors, params ActionEffect[] effects)
        {
            EnsureTurnState();

            var entry = new ActionLogEntry
            {
                Combat = combat,
                Turn = CurrentTurn,
                Actors = actors.ToArray(),
                Effects = effects
            };

            Entries.Add(entry);

            return entry;
        }

        public void ToggleEndTurn()
        {
            EnsureTurnState();
            CurrentTurn = IsEndTurnPending
                ? GetLastTurn()
                : GetLastTurn() + 1;
        }

        public void EnsureTurnState()
        {
            var previousTurn = 1;

            foreach (var entry in Entries)
            {
                if (entry.Turn <= 0)
                {
                    entry.Turn = previousTurn;
                }

                previousTurn = Math.Max(1, entry.Turn);
            }

            CurrentTurn = Math.Max(CurrentTurn, GetLastTurn());
        }

        public void SyncCurrentTurnAfterEntryMutation(bool keepEndTurnPending)
        {
            EnsureTurnState();

            var lastTurn = GetLastTurn();
            CurrentTurn = keepEndTurnPending && Entries.Count > 0
                ? lastTurn + 1
                : lastTurn;
        }

        private int GetLastTurn()
        {
            return Math.Max(1, Entries.LastOrDefault()?.Turn ?? 1);
        }
    }
}
