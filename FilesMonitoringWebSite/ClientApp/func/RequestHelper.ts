export const functions = {
    GetParams:(trackerid?: number, fileId?: number, userName?: string, fileFilter?: string, count?: number, page?: number):string => {
        var params = "";

        if (trackerid != null) {
            params = `?trackerid=${trackerid}`;
        }
        if (fileId != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `fileId=${fileId}`;
        }
        if(userName != null){
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `userName=${userName}`;
        }
        if (fileFilter != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `filter=${fileFilter}`;
        }
        if (count != null && page != null) {
            if (params.length != 0) {
                params += `&`;
            } else {
                params += `?`;
            }
            params += `count=${count}&page=${page}`;
        }

        return params;
    },

    TrySignInFetch: (userName:string, userPass:string): Promise<boolean | void>  => {
        // Only load data if it's something we don't already have (and are not already loading)
        return fetch(`/api/Authorization/SignIn?un=` + userName + `&pw=` + userPass, {
            method: 'POST',
            credentials: "same-origin"
        }).then(response => {
            if (response.status !== 200) return undefined;
            return response.json();
        }).then(data => {
            return data as boolean;
        }).catch(err => {
            console.log('Error :-S in user', err);
        });
    },

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