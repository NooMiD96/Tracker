import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router-dom';
import * as User from '../store/User';
import * as Tracker from '../store/Tracker';
import * as Exceptions from '../store/Exceptions';
import { ApplicationState } from '../store';
import TrackerListRender, { IDispatchProps as IDispatchPropsTrackerList, TrackerListProps } from '../components/tables/TrackerListRender';
import ExceptionListRender, { ExceptionListProps } from '../components/tables/ExceptionListRender';

interface IDispatchProps {
    ResetTrackerList: typeof Tracker.actionCreators.ResetTrackerList,
    SaveTrackerId: typeof Exceptions.actionCreators.SaveTrackerId,
    SaveUserName: typeof Exceptions.actionCreators.SaveUserName,
    ResetUserName: typeof Exceptions.actionCreators.ResetUserName,
    ResetExceptionList: typeof Exceptions.actionCreators.ResetExceptionList,
}

type UserExceptionsProps =
    User.UserState
    & IDispatchProps
    & RouteComponentProps<{}>;

class UserExceptions extends React.Component<UserExceptionsProps, {}> {
    componentDidUpdate() {
        let props = this.props;
        if (this.props.userName == null) {
            props.ResetTrackerList();
        }
    }

    public render() {
        const props = this.props;
        const user = {
            isAdministrating: props.isAdministrating,
            userType: props.userType,
            userName: props.userName,
        } as User.UserState;

        const someState = {
            user: user as User.UserState,
            funcs: {
                SaveTrackerId: props.SaveTrackerId,
                SaveUserName: props.SaveUserName,
                ResetUserName: props.ResetUserName,
            } as IDispatchPropsTrackerList
        }

        return this.props.userType != null &&
            <div className="row">
                {React.createElement(TrackerListRender, someState as TrackerListProps)}
                {React.createElement(ExceptionListRender, { user: user } as ExceptionListProps)}
            </div>
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.user,
    } as User.UserState;
}
const mapDispatchToProps = {
    ResetTrackerList: Tracker.actionCreators.ResetTrackerList,
    SaveTrackerId: Exceptions.actionCreators.SaveTrackerId,
    SaveUserName: Exceptions.actionCreators.SaveUserName,
    ResetUserName: Exceptions.actionCreators.ResetUserName,
    ResetExceptionList: Exceptions.actionCreators.ResetExceptionList,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(UserExceptions) as typeof UserExceptions;