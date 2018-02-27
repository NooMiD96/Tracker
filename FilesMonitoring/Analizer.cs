using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FilesMonitoring {
    class Analizer {
        static public string[] ignores;

        ConcurrentQueue<TrackerEvent> queue;
        List<TrackerEvent> globalNotDefinedEventList = new List<TrackerEvent>();
        static bool isWork = false;

        public Analizer(ConcurrentQueue<TrackerEvent> queue) {
            this.queue = queue;
        }

        public List<TrackerEvent> Analize() {
            if(isWork) return null;
            isWork = true;

            var returnList = new List<TrackerEvent>();
            var notDefinedEventList = new List<TrackerEvent>(globalNotDefinedEventList);
            bool needContinue;

            while(queue.Count > 0) {
                if(returnList.Count > 100 && notDefinedEventList.Count == 0) 
                    break;

                TrackerEvent evnt;
                while(!queue.TryDequeue(out evnt));

                NormalizePath(evnt);

                needContinue = IsNeedContinue(evnt);
                if(needContinue) 
                    continue;
                

                switch(evnt.EventName) {
                    //means copy or move big file or just create file
                    case TrackerEvents.Created:
                        if(Directory.Exists(evnt.FullName))
                            //todo: check all files in folder
                            break;
                        Create_Check(evnt, notDefinedEventList, returnList);

                        break;
                    //means all
                    case TrackerEvents.Changed:
                        Change_Check(evnt, notDefinedEventList, returnList);

                        break;
                    //means move file or just delete file
                    case TrackerEvents.Deleted:
                        Delete_Check(evnt, notDefinedEventList, returnList);

                        break;
                    //means rename
                    case TrackerEvents.Renamed:
                        returnList.Add(evnt);

                        break;
                    default:
                        return null;
                }
            }

            var createdNotOpens = notDefinedEventList.FindAll(itm => itm.EventName == TrackerEvents.Created);
            foreach(var item in createdNotOpens)
            {
                if(IsCanOpen(item.FullName))
                    returnList.Add(item);
                else
                    globalNotDefinedEventList.Add(item);
                notDefinedEventList.Remove(item);
            }

            var changedNotDefind = notDefinedEventList.FindAll(itm => itm.EventName == TrackerEvents.Changed);
            foreach(var item in changedNotDefind)
            {
                if(returnList.Find(itm => itm.FullName.Equals(item.FullName)) == null)
                {
                    returnList.Add(item);
                    notDefinedEventList.Remove(item);
                } else
                    notDefinedEventList.Remove(item);
            }

            foreach(var item in notDefinedEventList)
            {
                Console.WriteLine(item.FullName);
                Console.WriteLine(item.EventName);
            }

            if(returnList.Count == 0) {
                isWork = false;
                return null;
            } else {
                isWork = false;
                return returnList;
            }
        }
        public List<TrackerEvent> AnalizeDir(List<TrackerEvent> list)
        {
            var returnList = new List<TrackerEvent>();
            var notDefinedEventList = new List<TrackerEvent>();
            bool needContinue;

            foreach(var evnt in list)
            {
                if(returnList.Count > 100 && notDefinedEventList.Count == 0)
                    break;

                needContinue = IsNeedContinue(evnt);
                if(needContinue)
                    continue;


                switch(evnt.EventName)
                {
                    //means copy or move big file or just create file
                    case TrackerEvents.CreatedDir:
                        CreateDir_Check(evnt, notDefinedEventList, returnList);

                        break;
                    //means move file or just delete file
                    case TrackerEvents.DeletedDir:
                        DeleteDir_Check(evnt, notDefinedEventList, returnList);

                        break;
                    //means rename
                    case TrackerEvents.RenamedDir:
                        returnList.Add(evnt);

                        break;
                    default:
                        return null;
                }
            }

            var createdNotOpens = notDefinedEventList.FindAll(itm => itm.EventName == TrackerEvents.CreatedDir);
            foreach(var item in createdNotOpens)
            {
                if(IsCanOpen(item.FullName))
                    returnList.Add(item);
                else
                    globalNotDefinedEventList.Add(item);
                notDefinedEventList.Remove(item);
            }

            foreach(var item in notDefinedEventList)
            {
                Console.WriteLine(item.FullName);
                Console.WriteLine(item.EventName);
            }

            if(returnList.Count == 0)
            {
                isWork = false;
                return null;
            } else
            {
                isWork = false;
                return returnList;
            }
        }

        private void CreateDir_Check(TrackerEvent evnt, List<TrackerEvent> notDefinedEventList, List<TrackerEvent> returnList)
        {
            var deletedFiles = notDefinedEventList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.DeletedDir);

            //if before create event was delete event with same name
            //then it was moved a littel file
            if(deletedFiles.Count != 0)
            {
                //todo: need check on same files
                var item = deletedFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, evnt.FullName, item.FullName, TrackerEvents.MovedDir));
                notDefinedEventList.RemoveAll(itm => deletedFiles.Contains(itm));

                return;
            }

            deletedFiles = returnList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.DeletedDir);

            if(deletedFiles.Count != 0)
            {
                //todo: need check on same files
                var item = deletedFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, evnt.FullName, item.FullName, TrackerEvents.MovedDir));
                returnList.RemoveAll(itm => deletedFiles.Contains(itm));

                return;
            }

            returnList.Add(evnt);
        }
        private void DeleteDir_Check(TrackerEvent evnt, List<TrackerEvent> notDefinedEventList, List<TrackerEvent> returnList)
        {
            //if file was created and can be opened
            var createdFiles = returnList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.CreatedDir);
            if(createdFiles.Count != 0)
            {
                //todo: do any with many(if this can be)
                createdFiles[0].OldName = evnt.Name;
                createdFiles[0].OldFullName = evnt.FullName;
                createdFiles[0].EventName = TrackerEvents.MovedDir;
                return;
            }
            //if file was created and can't be opened
            createdFiles = notDefinedEventList.FindAll(itm => itm.Name.Equals(evnt.Name) && itm.EventName == TrackerEvents.CreatedDir);
            if(createdFiles.Count != 0)
            {
                //todo: do any with many(if this can be)
                var item = createdFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, item.FullName, evnt.FullName, TrackerEvents.MovedDir));
                notDefinedEventList.RemoveAll(itm => createdFiles.Contains(itm));
                return;
            }

            returnList.Add(evnt);
        }

        private void Change_Check(TrackerEvent evnt, List<TrackerEvent> notDefinedEventList, List<TrackerEvent> returnList)
        {
            if(!File.Exists(evnt.FullName))
                //change folder(skip)
                return;

            var item = notDefinedEventList.Find(itm => itm.FullName == evnt.FullName);
            if(item != null)
            {
                //was changed created file
                if(item.EventName == TrackerEvents.Created)
                {
                    bool isOpenend = IsCanOpen(evnt.FullName);
                    if(isOpenend)
                    {
                        returnList.Add(item);
                        notDefinedEventList.Remove(item);
                    }
                }
                return;
            }
            if(returnList.Find(itm => itm.FullName == evnt.FullName) != null)
                //was changed created or deleted file(skip)
                return;

            notDefinedEventList.Add(evnt);
        }
        private void Create_Check(TrackerEvent evnt, List<TrackerEvent> notDefinedEventList, List<TrackerEvent> returnList)
        {
            var deletedFiles = notDefinedEventList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.Deleted);
            
            //if before create event was delete event with same name
            //then it was moved a littel file
            if(deletedFiles.Count != 0)
            {
                //todo: need check on same files
                var item = deletedFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, evnt.FullName, item.FullName, TrackerEvents.Moved));
                notDefinedEventList.RemoveAll(itm => deletedFiles.Contains(itm));

                return;
            }

            deletedFiles = returnList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.Deleted);

            if(deletedFiles.Count != 0)
            {
                //todo: need check on same files
                var item = deletedFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, evnt.FullName, item.FullName, TrackerEvents.Moved));
                returnList.RemoveAll(itm => deletedFiles.Contains(itm));

                return;
            }

            //if dont was any events with this file
            //try open his 
            bool isOpenend = IsCanOpen(evnt.FullName);
            //if cant open, then file size big
            if(isOpenend)
                returnList.Add(evnt);
            else
                notDefinedEventList.Add(evnt);
            return;
        }
        private void Delete_Check(TrackerEvent evnt, List<TrackerEvent> notDefinedEventList, List<TrackerEvent> returnList)
        {
            //if file was created and can be opened
            var createdFiles = returnList.FindAll(itm => itm.Name == evnt.Name && itm.EventName == TrackerEvents.Created);
            if(createdFiles.Count != 0)
            {
                //todo: do any with many(if this can be)
                createdFiles[0].OldName = evnt.Name;
                createdFiles[0].OldFullName = evnt.FullName;
                createdFiles[0].EventName = TrackerEvents.Moved;
                return;
            }
            //if file was created and can't be opened
            createdFiles = notDefinedEventList.FindAll(itm => itm.Name.Equals(evnt.Name) && itm.EventName == TrackerEvents.Created);
            if(createdFiles.Count != 0)
            {
                //todo: do any with many(if this can be)
                var item = createdFiles[0];
                returnList.Add(new TrackerEvent(evnt.Name, evnt.Name, item.FullName, evnt.FullName, TrackerEvents.Moved));
                notDefinedEventList.RemoveAll(itm => createdFiles.Contains(itm));
                return;
            }

            returnList.Add(evnt);
        }

        private bool IsNeedContinue(TrackerEvent evnt)
        {
            var split = evnt.Name.Split('.');

            if(split.Length == 1)
            {
                if(ignores.Contains(split[0]) || ignores.Contains(evnt.Name))
                    return true;
            } else
            {
                if(ignores.Contains(split[0]) || ignores.Contains(split[1]) || ignores.Contains(evnt.Name))
                    return true;
            }

            foreach(var ignore in ignores)
                if(evnt.FullName.Contains(ignore))
                    return true;

            return false;
        }
        private bool IsCanOpen(string fullName)
        {
            try
            {
                var fi = new FileInfo(fullName).Open(FileMode.Open);
                fi.Close();
                return true;
            } catch
            {
                return false;
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
