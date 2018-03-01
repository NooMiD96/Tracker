using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace FilesMonitoring {
    public class TrackerEvent {
        
        public int id = 0;

        [Required]
        public string Name { get; set; }
        public string OldName { get; set; }
        [Required]
        public string FullName { get; set; }
        public string OldFullName { get; set; }
        [Required]
        public TrackerEvents EventName { get; set; }
        [Required]
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
        public byte[] Content { get; set; }
        [Required]
        public string UserName { get; set; } = Environment.UserName;
        [Required]
        public int TrackerEventInfoId { get; set; }

        [NotMapped]
        public bool sended = false;
        [NotMapped]
        public bool needSend = true;


        public TrackerEvent() {
            Name = OldName = FullName = OldFullName = "";
            EventName = TrackerEvents.Changed;
        }
        public TrackerEvent(string name, string fullName, TrackerEvents eventName) {
            Name = name;
            FullName = fullName;
            EventName = eventName;
        }
        public TrackerEvent(string name, string oldName, string fullName, string oldFullName, TrackerEvents eventName) {
            OldName = oldName;
            Name = name;
            FullName = fullName;
            OldFullName = oldFullName;
            EventName = eventName;
        }
        public TrackerEvent(TrackerEvent evnt) : this(evnt.Name, evnt.OldName, evnt.FullName, evnt.OldFullName, evnt.EventName) { }
    }
    public enum TrackerEvents {
        Changed,
        Created,
        Deleted,
        Renamed,
        Moved,
        CreatedDir,
        DeletedDir,
        RenamedDir,
        MovedDir,
    }
}
