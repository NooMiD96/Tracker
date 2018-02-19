import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../../store';
import * as Tracker from '../../store/Tracker';
import { UserState } from '../../store/User';
import Paginator from './components/Paginator';
import ViewCounter from './components/ViewCounter';
import AccessTrigerModalButton from '../buttons/AccessTrigerModalButton';
import EditUserModalButton from '../buttons/EditUserModalButton';
import AccessTrigerModal from '../modals/AccessTrigerModal';
import EditUserModal from '../modals/EditUserModal';

export interface IDispatchProps {
    SaveTrackerId: (trackerId: number) => void,
    SaveUserName: (userName: string) => void,
    ResetUserName: () => void,
}

export type TrackerListProps =
    Tracker.TrackerState
    & typeof Tracker.actionCreators
    & { user: UserState, funcs: IDispatchProps };

export class TrackerListRender extends React.Component<TrackerListProps, {}> {
    componentWillMount() {
        this.props.GetTrackerList();
    }
    componentDidMount(){
        this.modalContainer = document.getElementById('modal-container') as HTMLDivElement;
    }
    componentDidUpdate(prevProps: TrackerListProps) {
        let props = this.props;
        if (props.needGetData) {
            props.GetTrackerList(props.trackerListCountView, props.trackerListPage);
        }
    }

    modalContainer: HTMLDivElement;

    ButtonClick(item: any, type: string):void {
        var tmp;
        switch (type) {
            case 'AccessTrigerModal':
                tmp = React.createElement(AccessTrigerModal, {
                    EditOrAccessTrigger:(password?:string) => this.props.ChangeAccessUser(item.TrackerId,item.UserName,password),
                    IsCanAuthohorization:item.IsCanAuthohorization,
                    userName:item.UserName,
                    CloseClickHandler:() => this.DeleteModal('AccessTrigerModal'),
                });
                ReactDOM.render(tmp, this.modalContainer);

                break;

            case 'EditUserModal':
                tmp = React.createElement(EditUserModal, {
                    EditUserPassword:(password: string) => this.props.ChangeAccessUser(item.TrackerId, item.UserName, password),
                    userName:item.UserName,
                    CloseClickHandler:() => this.DeleteModal('EditUserModal'),
                });
                ReactDOM.render(tmp, this.modalContainer);
                
                break;

            default:
                break;
        }
    }
    DeleteModal(type:string):void{
        setTimeout(() => {
            (document.getElementById(type) as HTMLDivElement).remove()
        }, 350);
    }

    public render() {
        let props = this.props;
        if (props.trackerList == null) {
            return null;
        }
        let colSpanCount = 1;
        let templ;
        let header;
        if (props.user.isAdministrating) {
            colSpanCount += 2;
            header = [
                <th key={0}>UserName</th>,
                <th key={1}></th>,
            ]
            templ = props.trackerList.map((item, index) => {
            return <tr key={index} onClick={() => {props.funcs.SaveTrackerId(item.TrackerId); 
                    props.funcs.SaveUserName(item.UserName || '')}} className="clickable-tr">
                <td>
                    {item.TrackerId}
                </td>
                <td>
                    {item.UserName}
                </td>
                <td key={index}>
                    {
                        item.UserName
                            ? <AccessTrigerModalButton IsCanAuthohorization={item.IsCanAuthohorization}
                                onClickHandler={() => this.ButtonClick(item, 'AccessTrigerModal')}
                            />
                            : null
                    }
                    {
                        item.IsCanAuthohorization
                            ? <EditUserModalButton onClickHandler={() => this.ButtonClick(item, 'EditUserModal')}/>
                            : null
                    }
                </td>
            </tr>
        });
    } else {
        let uniqueList: Tracker.Tracker[] = [];
        uniqueList.push(props.trackerList[0]);
        if (props.trackerList != null) {
            for (var i = 1; i < props.trackerList.length; i++) {
                var j = 0;
                for (; j < uniqueList.length; j++) {
                    if (props.trackerList[i].TrackerId == uniqueList[j].TrackerId) break;
                }
                if (j == uniqueList.length) {
                    uniqueList.push(props.trackerList[i]);
                }
            }
        }

        templ = uniqueList.map((item, index) => {
            return <tr key={index} onClick={() => props.funcs.SaveTrackerId(item.TrackerId)} className="clickable-tr">
                <td>
                    {item.TrackerId}
                </td>
            </tr>
        });
    }

        return <div className='col-md-12'>
            <table className="tracker-table"><tbody>
                <tr className="table-header default-cursor">
                    <th>Trackers</th>
                    {header}
                </tr>
                {templ}
                <tr><td colSpan={colSpanCount}>
                    <Paginator currentPage={props.trackerListPage} CountView={props.trackerListCountView} movePageAction={props.MovePageTrackerList} />
                    <ViewCounter ViewCounterAction={props.ViewCountTrackerList} ViewNow={props.trackerListCountView} />
                </td></tr>
            </tbody></table>
        </div>;
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.tracker,
    } as Tracker.TrackerState;
}

const mapDispatchToProps = {
    ...Tracker.actionCreators,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(TrackerListRender) as typeof TrackerListRender;