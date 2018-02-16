using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesMonitoring {
    class Analizer {
        static public string[] ignores;
        static public string[] extensions;
        static public string[] split;

        ConcurrentQueue<TrackerEvent> queue;
        List<(TrackerEvent, TrackerEvents, DateTime)> notDefinedEventList = new List<(TrackerEvent, TrackerEvents, DateTime)>();

        static bool isWork = false;

        public Analizer(ConcurrentQueue<TrackerEvent> queue) {
            this.queue = queue;
        }

        public List<TrackerEvent> Analize() {
            if(isWork) return null;
            isWork = true;

            var returnList = new List<TrackerEvent>();
            List<(TrackerEvent, TrackerEvents)> notDefinedEventList = new List<(TrackerEvent, TrackerEvents)>();
            bool needContinue;
            while(queue.Count > 0) {
                if(returnList.Count > 1000) {
                    break;
                }
                needContinue = false;
                TrackerEvent evnt;
                while(!queue.TryDequeue(out evnt)) ;

                NormalizePath(evnt);

                split = evnt.Name.Split('.');
                if(split.Length == 1) {
                    if(ignores.Contains(split[0]) || ignores.Contains(evnt.Name)) {
                        continue;
                    }
                } else {
                    if(ignores.Contains(split[0]) || ignores.Contains(split[1]) || ignores.Contains(evnt.Name)) {
                        continue;
                    }
                }

                foreach(var ignore in ignores) {
                    if(evnt.FullName.Contains(ignore)) {
                        needContinue = true;
                        break;
                    }
                }
                if(needContinue) {
                    continue;
                }

                switch(evnt.EventName) {
                    //means copy or move big file or just create file
                    case TrackerEvents.Created:
                        if(Directory.Exists(evnt.FullName)) {
                            continue;
                        }

                        var tmpItem = notDefinedEventList.FindAll(itm => itm.Item1.Name == evnt.Name && itm.Item2 == TrackerEvents.Moved);
                        if(tmpItem.Count != 0) {
                            var item = tmpItem[0].Item1;
                            returnList.Add(new TrackerEvent(item.Name, evnt.Name, evnt.FullName, item.FullName, TrackerEvents.Moved));

                            notDefinedEventList.RemoveAll(itm => tmpItem.Contains(itm));
                        } else {
                            bool isOpen = false;
                            try {
                                var fi = new FileInfo(evnt.FullName).Open(FileMode.Open);
                                isOpen = true;
                                fi.Close();
                            } catch { }

                            if(isOpen) {
                                evnt.EventName = TrackerEvents.Created;
                                returnList.Add(evnt);
                            } else {

                                notDefinedEventList.Add((evnt, TrackerEvents.Moved));
                            }
                        }
                        continue;
                    //means all
                    case TrackerEvents.Changed:
                        if(Directory.Exists(evnt.FullName) || !File.Exists(evnt.FullName)) {
                            continue;
                        }
                        if(notDefinedEventList.Find(itm => itm.Item1.FullName == evnt.FullName).Item1 != null) {
                            continue;
                        }
                        if(notDefinedEventList.Find(itm => itm.Item1.Name == evnt.Name && itm.Item2 == TrackerEvents.Moved).Item1 != null) {
                            continue;
                        }
                        if(returnList.Find(itm => itm.FullName == evnt.FullName) != null) {
                            continue;
                        }
                        notDefinedEventList.Add((evnt, TrackerEvents.Changed));
                        continue;
                    //means move file or just delete file
                    case TrackerEvents.Deleted:
                        var tempItem = returnList.Find(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.Created);
                        if(tempItem != null) {
                            tempItem.OldName = evnt.Name;
                            tempItem.OldFullName = evnt.FullName;
                            tempItem.EventName = TrackerEvents.Moved;
                            continue;
                        }

                        var tmpItem1 = notDefinedEventList.Find(itm => itm.Item1.EventName != TrackerEvents.Deleted && itm.Item2 == TrackerEvents.Moved && itm.Item1.Name == evnt.Name);
                        var tmpItem2 = notDefinedEventList.FindAll(itm => itm.Item2 == TrackerEvents.Changed && itm.Item1.Name == evnt.Name);

                        if(tmpItem1.Item1 != null) {
                            var item = tmpItem1.Item1;
                            returnList.Add(new TrackerEvent(item.Name, evnt.Name, item.FullName, evnt.FullName, TrackerEvents.Moved));
                            notDefinedEventList.Remove(tmpItem1);
                            if(tmpItem2.Count != 0) {
                                notDefinedEventList.RemoveAll(itm => tmpItem2.Contains(itm));
                            }
                            continue;
                        }
                        notDefinedEventList.Add((evnt, TrackerEvents.Moved));
                        continue;
                    //means rename
                    case TrackerEvents.Renamed:
                        returnList.Add(evnt);
                        break;

                    default:
                        return null;
                }
            }

            var tmpList = notDefinedEventList.FindAll(itm => itm.Item2 == TrackerEvents.Changed
                && (returnList.FirstOrDefault(rtrn => rtrn.Name == itm.Item1.Name) != null));

            if(tmpList.Count != 0) {
                notDefinedEventList.RemoveAll(itm => tmpList.Contains(itm));
            }

            if(notDefinedEventList.Count != 0) {
                foreach(var item in notDefinedEventList) {
                    switch(item.Item2) {
                        case TrackerEvents.Changed:
                            var fndChange = this.notDefinedEventList.FindAll(itm => itm.Item1.FullName == item.Item1.FullName);
                            if(fndChange.Count == 0) {
                                returnList.Add(item.Item1);
                            }
                            break;
                        case TrackerEvents.Moved:
                            var fndMove = this.notDefinedEventList.FindAll(itm => itm.Item1.Name == item.Item1.Name && itm.Item1.EventName != TrackerEvents.Deleted && item.Item1.EventName != TrackerEvents.Deleted);
                            if(fndMove.Count != 0) {
                                var evnt = fndMove[0].Item1;
                                if(item.Item1.EventName == TrackerEvents.Deleted) {
                                    evnt.OldFullName = item.Item1.FullName;
                                    evnt.OldName = item.Item1.Name;
                                } else {
                                    evnt.OldFullName = evnt.FullName;
                                    evnt.OldName = evnt.Name;

                                    evnt.FullName = item.Item1.FullName;
                                    evnt.Name = item.Item1.Name;
                                }
                                evnt.EventName = TrackerEvents.Moved;

                                returnList.Add(evnt);
                                this.notDefinedEventList.RemoveAll(itm => fndMove.Contains(itm));
                                break;
                            }

                            //fndMove = returnList.FindAll(itm => notDefinedEventList.Find())


                            this.notDefinedEventList.Add((item.Item1, item.Item2, DateTime.Now));
                            break;
                        default:
                            break;
                    }
                }
            }

            //check can open file 
            List<(TrackerEvent, TrackerEvents, DateTime)> listToDelete = new List<(TrackerEvent, TrackerEvents, DateTime)>();
            foreach(var item in this.notDefinedEventList) {
                var evnt = item.Item1;
                if(evnt.EventName == TrackerEvents.Created) {
                    bool isOpen = false;
                    try {
                        var fi = new FileInfo(evnt.FullName).Open(FileMode.Open);
                        isOpen = true;
                        fi.Close();
                    } catch { }

                    if(isOpen) {
                        //evnt.EventName = TrackerEvents.Created;
                        returnList.Add(evnt);
                        listToDelete.Add(item);
                    } else {
                        continue;
                    }
                } else {
                    var fndFromReturnListMove = returnList.FindAll(itm => itm.FullName == item.Item1.FullName);
                    if(fndFromReturnListMove.Count != 0) {
                        var fndEvnt = fndFromReturnListMove[0];
                        fndEvnt.EventName = TrackerEvents.Moved;
                        fndEvnt.OldFullName = item.Item1.FullName;

                        listToDelete.Add(item);
                        break;
                    }
                }
                var tmpTime = item.Item3;
                if(item.Item3.AddSeconds(10) < DateTime.Now) {
                    listToDelete.Add(item);
                    returnList.Add(item.Item1);
                }
            }
            this.notDefinedEventList.RemoveAll(itm => listToDelete.Contains(itm));

            isWork = false;
            if(returnList.Count == 0) {
                return null;
            } else {
                return returnList;
            }
        }

        private void NormalizePath(TrackerEvent evnt) {
            int indexOfLastSlesh;

            indexOfLastSlesh = evnt.FullName.LastIndexOf('\\');
            evnt.Name = evnt.FullName.Substring(indexOfLastSlesh + 1);

            if(evnt.EventName == TrackerEvents.Renamed) {
                indexOfLastSlesh = evnt.OldFullName.LastIndexOf('\\');
                evnt.OldName = evnt.OldFullName.Substring(indexOfLastSlesh + 1);
            } 
        }
    }
}
