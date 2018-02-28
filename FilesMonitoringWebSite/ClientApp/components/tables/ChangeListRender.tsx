import * as React from 'react';
import { connect } from 'react-redux';
import { RouteComponentProps } from 'react-router-dom';
import { ApplicationState }  from '../../store';
import * as Change from '../../store/Change';
import * as Helper from '../../func/RequestHelper';
import { FormateTime } from '../../func/TimeFormater';
import { UserState } from 'ClientApp/store/User';
import Paginator from './components/Paginator';
import ViewCounter from './components/ViewCounter';

export type ChangeListProps =
    Change.ChangeState
    & typeof Change.actionCreators
    & {user: UserState};

export class ChangeListRender extends React.Component<ChangeListProps, {}> {
    componentDidUpdate(prevProps: ChangeListProps) {
        let props = this.props;
        if(props.needGetData) {
            props.GetChangeList(props.fileId, props.changeListCountView, props.changeListPage);
        }
    }
    public render() {
        let props = this.props;

        if (this.props.changeList == null) {
            return null;
        }
        
        var templ = this.props.changeList.map((item, index)=>{
            return <tr key={index}>
                <td>
                    {FormateTime(item.DateTime)}
                </td>
                <td>
                    {Change.ChangeEvents[item.EventName]}
                </td>
                <td>
                    {
                        (item.EventName == Change.ChangeEvents.Changed || item.EventName == Change.ChangeEvents.Created) &&
                            <a className="clickable-link" onClick={() => Helper.functions.DownloadFile(item.Id.toString())}>Download</a>
                    }
                </td>
                <td>
                    {item.UserName}
                </td>
                <td>
                    {item.OldName}
                </td>
                <td>
                    {item.OldFullName}
                </td>
            </tr>
        });

        return <div className='col-md-12'>
            <table><tbody>
                <tr>
                    <th>Date Time</th>
                    <th>Event Name</th>
                    <th>Link to download</th>
                    <th>User Name</th>
                    <th>Old Name</th>
                    <th>Old Full Name</th>
                </tr>
                {templ}
                <tr><td colSpan={6}>
                    <Paginator currentPage={props.changeListPage} CountView={props.changeListCountView} movePageAction={this.props.MovePageChangeList}/>
                    <ViewCounter ViewCounterAction={props.ViewCountChangeList} ViewNow={this.props.changeListCountView}/>
                </td></tr>
            </tbody></table>
        </div>;
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.change,
    } as Change.ChangeState;
}

const mapDispatchToProps = {
    ...Change.actionCreators,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(ChangeListRender) as typeof ChangeListRender;