import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.
export interface UserState {
    userType: string | null,
    userName: string | null,
    tableScale: number,
    isAdministrating: boolean,
}
// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.
// Use @typeName and isActionType for type detection that works even after serialization/deserialization.

interface GetUserInfoAction {
    type: 'GET_USER_INFO',
    userType: string | null,
    userName: string | null,
}
interface SignOutAction {
    type: 'SIGN_OUT',
}
interface AdminTrigger {
    type: 'ADMIN_TRIGGER',
}
// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = GetUserInfoAction | SignOutAction | AdminTrigger;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).
export const functions = {
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
}

export const actionCreators = {
    GetUserInfo: (): AppThunkAction<GetUserInfoAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        let fetchTask = fetch(`/api/Authorization/GetUserInfo`, {
            method: 'GET',
            credentials: "same-origin"
        }).then(response => {
            if (response.status !== 200) return undefined;
            return response.json();
        }).then(data => {
            dispatch({ type: 'GET_USER_INFO', userType: data.userType, userName: data.userName });
        }).catch(err => {
            console.log('Error :-S in user', err);
        });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
    },
    SignOut: (): AppThunkAction<SignOutAction> => (dispatch, getState) => {
        let fetchTask = fetch(`/api/Authorization/SignOut`, {
            method: 'PUT',
            credentials: "same-origin"
        }).then(response => {
            dispatch({ type: 'SIGN_OUT' });
        }).catch(err => {
            console.log('Error :-S in user', err);
        });
        addTask(fetchTask);
    },
    AdminTrigger: () => <AdminTrigger>{ type: 'ADMIN_TRIGGER' },
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.
const unloadedState: UserState = { userName: null, userType: null, tableScale: 6, isAdministrating: false };

export const reducer: Reducer<UserState> = (state: UserState, action: KnownAction) => {
    switch (action.type) {
        case 'SIGN_OUT':
            return unloadedState;

        case 'GET_USER_INFO':
            var scale;
            
            if(action.userType == "Admin"){
                scale = 4;
            }else{
                scale = 6;                
            }

            return {
                userName: action.userName,
                userType: action.userType,   
                tableScale: scale,
                isAdministrating: false,
            };

        case 'ADMIN_TRIGGER':
            return {
                userName: state.userName,
                userType: state.userType,   
                tableScale: state.tableScale,
                isAdministrating: !state.isAdministrating,
            }

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    // For unrecognized actions (or in cases where actions have no effect), must return the existing state
    //  (or default initial state if none was supplied)
    return state || unloadedState;
};