using System;
using System.Threading.Tasks;
using System.Linq;
using ReactiveUI;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.GUI.ViewModels
{
    public class AvailableVersionFetchingProgressViewModel : ViewModelBase
    {
        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            private set => this.RaiseAndSetIfChanged(ref _totalCount, value);
        }

        private int _inProgress;
        public int InProgress
        {
            get => _inProgress;
            private set => this.RaiseAndSetIfChanged(ref _inProgress, value);
        }

        private int _failed;
        public int Failed
        {
            get => _failed;
            private set => this.RaiseAndSetIfChanged(ref _failed, value);
        }

        private ObservableAsPropertyHelper<int> _completed;
        public int Completed => _completed.Value;

        private ObservableAsPropertyHelper<string> _label;
        public string ProgressLabel => _label.Value;

        private object _lock;


        public AvailableVersionFetchingProgressViewModel()
        {
            _lock = new object();
            Clear();

            _completed = this
                .WhenAnyValue(x => x.InProgress, x => x.Failed, x => x.TotalCount, 
                    (inProgress, failed, total) => total - (inProgress + failed))
                .ToProperty(this, x => x.Completed, out _completed);

            _label = this
                .WhenAnyValue(x => x.Completed, x => x.TotalCount,
                    (completed, total) =>
                    {
                        if (completed < total)
                        {
                            return $"Fetching latest package versions... ({completed}/{total})";
                        }
                        return $"Fetched latest package versions... ({completed}/{total})";
                    })
                .ToProperty(this, x => x.ProgressLabel, out _label);
                
        }

        public void AddNewInProgress()
        {
            lock (_lock)
            {
                _inProgress++;
                TotalCount++;
                this.RaisePropertyChanged(nameof(InProgress));
                // note: manually raising the first updated property to ensure that every notified subscriber always has consistent counts
            }
        }

        public void MarkOneAsCompleted()
        {
            lock (_lock)
            {
                if (InProgress > 0)
                    InProgress--;
            }
        }

        public void MarkOneAsFailed()
        {
            lock (_lock)
            {
                if (InProgress > 0)
                {
                    _failed++;
                    InProgress--;
                    this.RaisePropertyChanged(nameof(Failed));
                    // note: manually raising the first updated property to ensure that every notified subscriber always has consistent counts
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _inProgress = 0;
                _totalCount = 0;
                _failed = 0;
                this.RaisePropertyChanged(nameof(InProgress));
                this.RaisePropertyChanged(nameof(TotalCount));
                this.RaisePropertyChanged(nameof(Failed));
            }
        }
    }
}