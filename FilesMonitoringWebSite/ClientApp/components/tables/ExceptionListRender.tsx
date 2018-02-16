import * as React from 'react';
import { connect } from 'react-redux';
import { ApplicationState }  from '../../store';
import { UserState } from '../../store/User';
import * as ExceptionState from '../../store/Exceptions';
import Paginator from './components/Paginator';
import ViewCounter from './components/ViewCounter';
import { FormateTime } from '../../func/TimeFormater'

export type ExceptionListProps =
    ExceptionState.ExceptionState
    & typeof ExceptionState.actionCreators
    & {user: UserState};

export class ExceptionListRender extends React.Component<ExceptionListProps, {}> {
    componentDidUpdate(prevProps: ExceptionListProps) {
        let props = this.props;
        if(props.needGetData) {
            props.GetExceptionList(props.trackerId, props.userName, props.exceptionListCountView, props.exceptionListPage);
        }
    }

    public render() {
        let props = this.props;

        if (props.exceptionList == null) {
            return null;
        }

        var templ = props.exceptionList.map((item, index)=>{
            return <tr key={index}>
                <td>
                    {FormateTime(item.DateTime)}
                </td>
                <td>
                    {item.ExceptionInner}
                </td>
            </tr>
        });

        return <div className='col-md-12'>
            {
                props.userName 
                    ? <div style={{margin: '10px 0px 10px 0px'}}>
                        <p style={{display: 'inline-block', marginRight: '10px'}}>{props.userName}</p>
                        <button type="button" className={"btn btn-default"} 
                            onClick={props.DeleteUserName}><span className="glyphicon glyphicon-remove" aria-hidden="true"></span>
                        </button>
                    </div>
                    : null
            }
            <table><tbody>
                <tr>
                    <th>Date</th>
                    <th>Exception inner</th>
                </tr>
                    {templ}
                <tr><td colSpan={2}>
                    <Paginator currentPage={props.exceptionListPage} CountView={props.exceptionListCountView} movePageAction={this.props.MovePageExceptionList}/>
                    <ViewCounter ViewCounterAction={props.ViewCountExceptionList} ViewNow={this.props.exceptionListCountView} />
                </td></tr>
            </tbody></table>
        </div>;
    }
}

function mapStateToProps(state: ApplicationState) {
    return {
        ...state.exception,
    } as ExceptionState.ExceptionState;
}

const mapDispatchToProps = {
    ...ExceptionState.actionCreators,
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(ExceptionListRender) as typeof ExceptionListRender;