export const functions = {
    fetchTask: (type: string, method:string, value: string): Promise<any | void> => {
        return fetch(`/api/Tracker/` + type + value, {
            method: method,
            credentials: "same-origin",
        }).then(response => {
            if (response.status !== 200) return undefined;
            return response.json();
        }).catch(err => {
            console.log('Error :-S in change list', err);
        });
    },
    DownloadFile:(changeId: string): void =>{
        window.open(`/api/Tracker/GetFile?changeId=` + changeId);
    },
    fetchAdminTask: (type: string, method: string, value: string): Promise<any | void> => {
        if (method == 'GET') {
            return fetch(`/api/Admin/` + type + value, {
                method: method,
                credentials: "same-origin",
            }).then(response => {
                if (response.status !== 200) return undefined;
                return response.json();
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });
        } else {
            return fetch(`/api/Admin/` + type, {
                method: method,
                credentials: "same-origin",
                body: value,
                headers: new Headers({
                    'Content-Type': 'application/json'
                })
            }).then(response => {
                if (response.status !== 200) return undefined;
                return response.json();
            }).catch(err => {
                console.log('Error :-S in change list', err);
            });
        }
    }
}