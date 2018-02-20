import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router-dom';
import * as User from '../store/User';
import * as Tracker from '../store/Tracker';
import * as File from '../store/File';
import * as Change from '../store/Change';
import { ApplicationState } from '../store';
import TrackerListRender, { TrackerListProps, IDispatchProps as IDispatchPropsTrackerList } from '../components/tables/TrackerListRender';
import FileListRender, { FileListProps } from '../components/tables/FileListRender';
import ChangeListRender, { ChangeListProps } from '../components/tables/ChangeListRender';
import BoundTriggerButton from '../components/buttons/BoundTriggerButton';
import SearchField from '../components/SearchField';

interface IDispatchProps {
    ResetTrackerList: typeof Tracker.actionCreators.ResetTrackerList,
    ResetChangeList: typeof Change.actionCreators.ResetChangeList,
    ResetFileList: typeof File.actionCreators.ResetFileList,
    SaveFileFilter: typeof File.actionCreators.SaveFileFilter,
    SaveTrackerId: typeof File.actionCreators.SaveTrackerId,
    SaveUserName: typeof File.actionCreators.SaveUserName,
    ResetUserName: typeof File.actionCreators.ResetUserName,
    ResetFileFilter: typeof File.actionCreators.ResetFileFilter,
    BoundTrigger: typeof User.actionCreators.BoundTrigger,
}

type UserProps =
    User.UserState
    & IDispatchProps
    & RouteComponentProps<{}>;

class UserChanges extends React.Component<UserProps, {}> {
    componentDidUpdate() {
        let props = this.props;
        if (this.props.userName == null) {
            props.ResetTrackerList();
            props.ResetFileList();
            props.ResetChangeList();
        }
    }

    public render() {
        let props = this.props;
        let user = {
            isBondUserName: props.isBondUserName,
            userType: props.userType,
            userName: props.userName,
        } as User.UserState;

        const someState ={
            user: user as User.UserState,
            funcs : {
                SaveTrackerId: props.SaveTrackerId,
                SaveUserName: props.SaveUserName,
                ResetUserName: props.ResetUserName,
                BoundTrigger: props.BoundTrigger,
            } as IDispatchPropsTrackerList
        }

        return this.props.userType != null &&
            <div className="row">
                <SearchField SaveFilter={props.SaveFileFilter} ResetFilter={props.ResetFileFilter} ResetChangeList={props.ResetChangeList}/>
                <BoundTriggerButton BoundTrigger={props.BoundTrigger} userType={props.userType} isBoundUserName={props.isBondUserName} />
                {React.createElement(TrackerListRender, someState as TrackerListProps)}
                {React.createElement(FileListRender, { user: user } as FileListProps)}
                {React.createElement(ChangeListRender, { user: user } as ChangeListProps)}
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
    ResetChangeList: Change.actionCreators.ResetChangeList,
    ResetFileList: File.actionCreators.ResetFileList,
    SaveFileFilter: File.actionCreators.SaveFileFilter,
    SaveTrackerId: File.actionCreators.SaveTrackerId,
    SaveUserName: File.actionCreators.SaveUserName,
    ResetUserName: File.actionCreators.ResetUserName,
    ResetFileFilter: File.actionCreators.ResetFileFilter,
    BoundTrigger: User.actionCreators.BoundTrigger,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(UserChanges) as typeof UserChanges;