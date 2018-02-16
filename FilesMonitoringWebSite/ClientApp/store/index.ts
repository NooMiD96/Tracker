import * as User from './User';
import * as Tracker from './Tracker';
import * as File from './File';
import * as Change from './Change';
import * as Exception from './Exceptions';
// The top-level state object
export interface ApplicationState {
    user: User.UserState,
    tracker: Tracker.TrackerState,
    file: File.FileState,
    change: Change.ChangeState,
    exception: Exception.ExceptionState,
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
    user: User.reducer,
    tracker: Tracker.reducer,
    file: File.reducer,
    change: Change.reducer,
    exception: Exception.reducer,    
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}
