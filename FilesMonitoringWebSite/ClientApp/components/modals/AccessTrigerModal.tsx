import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';
import NavMenu from '../NavMenu';
import * as Tracker from '../../store/Tracker';
import * as $ from 'jquery';

interface InputProps {
    EditOrAccessTrigger: (password?: string) => void,
    IsCanAuthohorization?: boolean,
    userName?:string,
    CloseClickHandler: () => void,
}

export default class AccessTrigerModal extends React.Component<InputProps, {}> {
    modal: any;

    componentDidMount(){
        this.modal = $('#AccessTrigerModal');
        this.modal.modal('show');
    }

    CreateAccess = () => {
        var DOM_inputPW = ReactDOM.findDOMNode(this.refs.inputPW) as HTMLInputElement;
        if(DOM_inputPW == null) {
            this.props.EditOrAccessTrigger();            
            this.modal.modal('toggle');
            return true;
        }
        var isEmpty = false;

        if (DOM_inputPW.value == "") {
            DOM_inputPW.style.setProperty("border-color", "red");
            isEmpty = true;
        } else {
            DOM_inputPW.setAttribute("style", "border-color: initial;");
        }

        if(!isEmpty) {
            this.props.EditOrAccessTrigger(DOM_inputPW.value);
            DOM_inputPW.value = "";
            this.modal.modal('toggle');
            return true;
        }
        return false;
    }
    DeleteAccess = () => {
        this.props.EditOrAccessTrigger();

        this.modal.modal('toggle');
    }

    CloseButtonHandler = () => {
        this.modal.modal('hide');
    }

    public render() {
        let props = this.props;
        let header, body, buttonHandler;
        if(props.IsCanAuthohorization){
            header = <h3>Denied access to {props.userName}?</h3>
            body = null;
            buttonHandler = this.DeleteAccess;
        }else{
            header = <h3>Create access to {props.userName}?</h3>
            body = <div className="modal-body">
                <input type="password" className="modalInputPassword" placeholder="Enter Password" ref="inputPW" />
                <p className="validateModalInputPassword" ref="input_pw_error"></p>
            </div>
            buttonHandler = this.CreateAccess;
        }
        return <div className="modal fade" id="AccessTrigerModal" role="dialog" data-backdrop={false}>
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <button type="button" className="close" aria-label="Close" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}><span aria-hidden="true">&times;</span></button>
                        <h2 className="modal-title" id="CreateUserLabel">Warning:</h2>
                        {header}
                    </div>
                    {body}
                    <div className="modal-footer">
                        <button type="button" className="btn btn-default" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}>No</button>
                        <button type="button" className="btn btn-primary" onClick={() => {
                            this.CreateAccess() ? this.props.CloseClickHandler() : null
                        }}>Yes</button>
                    </div>
                </div>
            </div>
        </div>
    }
}
