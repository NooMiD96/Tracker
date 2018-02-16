import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { connect } from 'react-redux';
import { ApplicationState }  from '../../store';
import * as File from '../../store/File';
import * as Change from '../../store/Change';
import { UserState } from '../../store/User';
import Paginator from './components/Paginator';
import ViewCounter from './components/ViewCounter';
import { FormateTime } from '../../func/TimeFormater'
import EditDeleteTimeModalButton from '../buttons/EditDeleteTimeModalButton';
import EditDeleteTimeModal from '../modals/EditDeleteTimeModal';

interface IDispatchProps {
    GetChangeList: typeof Change.actionCreators.GetChangeList,
    ResetChangeList: typeof Change.actionCreators.ResetChangeList,
}

export type FileListProps =
    File.FileState
    & typeof File.actionCreators
    & IDispatchProps
    & {user: UserState};

export class FileListRender extends React.Component<FileListProps, {}> {
    componentDidMount(){
        this.modalContainer = document.getElementById('modal-container') as HTMLDivElement;

        if(this.props.user.userType == "User"){
            this.props.GetFileList();
        }
    }
    componentDidUpdate(prevProps: FileListProps) {
        let props = this.props;
        props.ResetChangeList();
        if(props.needGetData) {
            props.GetFileList(props.trackerId, props.userName, props.fileFilter, props.fileListCountView, props.fileListPage);
        }
    }

    modalContainer: HTMLDivElement;

    ButtonClick(item: any):void {
        var tmp = React.createElement(EditDeleteTimeModal, { FileId:item.FileId, isNeedDelete:item.IsNeedDelete, key:item.FileId,
            EditDeleteTime:(date:Date, isNeedDelete:boolean) => this.props.EditDeleteTime(item.FileId, date, isNeedDelete),
            CloseClickHandler: this.DeleteModal,
        });
        ReactDOM.render(tmp, this.modalContainer);
    }
    DeleteModal():void{
        setTimeout(() => (document.getElementById('EditDeleteTimeModal') as HTMLDivElement).remove(), 1000);
    }

    public render() {
        let props = this.props;

        if (props.fileList == null) {
            return null;
        }

        var templ = props.fileList.map((item, index)=>{
            return <tr key={index} onClick={() => props.GetChangeList(item.FileId)} className="clickable-tr">
                <td>
                    {item.FileName}
                </td>
                <td>
                    {item.FullName}
                </td>
                <td>
                    {
                        item.IsWasDeletedChange
                            ? "Yes"
                            : "No"
                    }
                </td>
                <td>
                    {
                        item.RemoveFromDbTime != null &&
                            <div>
                                <p style={{display: "inline-block"}}>{FormateTime(item.RemoveFromDbTime, false)}</p>
                                {props.user.userType == "Admin" ? <EditDeleteTimeModalButton onClickHandler={() => this.ButtonClick(item)}/> : null}
                            </div> 
                    }
                </td>
            </tr>
        });

        return <div className='col-md-12'>
            {
                props.userName 
                    ? <div style={{margin: '10px 0px 10px 0px'}}>
                        <p style={{display: 'inline-block', marginRight: '10px'}}>{props.userName}</p>
                        <button type="button" className={"btn btn-default"} 
                            onClick={props.ResetUserName}><span className="glyphicon glyphicon-remove" aria-hidden="true"></span>
                        </button>
                    </div>
                    : null
            }
            <table><tbody>
                <tr>
                    <th>File Name</th>
                    <th>Full Name</th>
                    <th>Was Deleted</th>
                    <th>Remove From Db Time</th>
                </tr>
                    {templ}
                <tr><td colSpan={4}>
                    <Paginator currentPage={props.fileListPage} CountView={props.fileListCountView} movePageAction={this.props.MovePageFileList}/>
                    {
                        props.fileFilter != null &&
                            <button type="button" className={"btn btn-default right"} 
                                onClick={props.ResetFileFilter}><span className="glyphicon glyphicon-refresh" aria-hidden="true"></span>
                            </button>
                    }
                    <ViewCounter ViewCounterAction={props.ViewCountFileList} ViewNow={this.props.fileListCountView} />
                </td></tr>
            </tbody></table>
        </div>;
    }

}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.file,
    } as File.FileState;
}

const mapDispatchToProps = {
    ...File.actionCreators,
    GetChangeList: Change.actionCreators.GetChangeList,
    ResetChangeList: Change.actionCreators.ResetChangeList,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(FileListRender) as typeof FileListRender;