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
import SearchField from '../components/tables/SearchField';

interface IDispatchProps {
    ResetTrackerList: typeof Tracker.actionCreators.ResetTrackerList,
    ResetFileList: typeof File.actionCreators.ResetFileList,
    SaveFileFilter: typeof File.actionCreators.SaveFileFilter,
    GetFileList: typeof File.actionCreators.GetFileList,
    ResetChangeList: typeof Change.actionCreators.ResetChangeList,
    ResetFileFilter: typeof File.actionCreators.ResetFileFilter,
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
            isAdministrating: props.isAdministrating,
            userType: props.userType,
            userName: props.userName,
        } as User.UserState;

        const someState ={
            user: user as User.UserState,
            funcs : {
                GetSomeList: props.GetFileList,
                ResetSomeList: props.ResetFileList
            } as IDispatchPropsTrackerList
        }

        return this.props.userType != null
            ? <div className="row">
                <SearchField SaveFilter={props.SaveFileFilter} ResetFilter={props.ResetFileFilter} ResetChangeList={props.ResetChangeList}/>
                {React.createElement(TrackerListRender, someState as TrackerListProps)}
                {React.createElement(FileListRender, { user: user } as FileListProps)}
                {React.createElement(ChangeListRender, { user: user } as ChangeListProps)}
            </div>
            : null
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.user,
    } as User.UserState;
}
const mapDispatchToProps = {
    ResetTrackerList: Tracker.actionCreators.ResetTrackerList,
    ResetFileList: File.actionCreators.ResetFileList,
    SaveFileFilter: File.actionCreators.SaveFileFilter,
    ResetFileFilter: File.actionCreators.ResetFileFilter,
    GetFileList: File.actionCreators.GetFileList,
    ResetChangeList: Change.actionCreators.ResetChangeList,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(UserChanges) as typeof UserChanges;