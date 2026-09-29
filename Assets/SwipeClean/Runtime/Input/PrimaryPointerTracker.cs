namespace SwipeClean.Input
{
    /// <summary>Accepts one pointer lifecycle and rejects secondary or orphaned samples.</summary>
    public sealed class PrimaryPointerTracker
    {
        private int _activePointerId = -1;
        private bool _activePointerIsBlocked;

        public bool HasActivePointer => _activePointerId >= 0;
        public int ActivePointerId => _activePointerId;

        public bool TryAccept(in RawPointerSample sample, bool beganOverUi)
        {
            if (sample.Phase == PointerPhase.Began)
            {
                if (HasActivePointer)
                {
                    return false;
                }

                _activePointerId = sample.PointerId;
                _activePointerIsBlocked = beganOverUi;
                return !_activePointerIsBlocked;
            }

            if (sample.PointerId != _activePointerId)
            {
                return false;
            }

            var accepted = !_activePointerIsBlocked;
            if (sample.Phase == PointerPhase.Ended || sample.Phase == PointerPhase.Cancelled)
            {
                Reset();
            }

            return accepted;
        }

        public void Cancel() => Reset();

        private void Reset()
        {
            _activePointerId = -1;
            _activePointerIsBlocked = false;
        }
    }
}

