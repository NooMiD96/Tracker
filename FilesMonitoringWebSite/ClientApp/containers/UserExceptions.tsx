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
    GetExceptionList: typeof Exceptions.actionCreators.GetExceptionList,
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
                GetSomeList: props.GetExceptionList,
                ResetSomeList: props.ResetExceptionList,
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
    GetExceptionList: Exceptions.actionCreators.GetExceptionList,
    ResetExceptionList: Exceptions.actionCreators.ResetExceptionList,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(UserExceptions) as typeof UserExceptions;