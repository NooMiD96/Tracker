import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';
import NavMenu from '../NavMenu';
import * as $ from 'jquery';

interface InputProps {
    EditUserPassword: (password: string) => any,
    userName: string | undefined,
    CloseClickHandler: () => void,
}

export default class EditUserModal extends React.Component<InputProps, {}> {
    modal: any;
    
    componentDidMount(){
        this.modal = $('#EditUserModal');
        this.modal.modal('show');
    }

    EditHandler = () => {
        var isEmpty = false;
        var DOM_inputPW = ReactDOM.findDOMNode(this.refs.inputPW) as HTMLInputElement;

        if (DOM_inputPW.value == "") {
            DOM_inputPW.style.setProperty("border-color", "red");
            isEmpty = true;
        } else {
            DOM_inputPW.setAttribute("style", "border-color: initial;");
        }
        if (!isEmpty) {
            let props = this.props;
            props.EditUserPassword(DOM_inputPW.value);

            this.modal.modal('hide');

            DOM_inputPW.value = "";
            return true;
        }
    }
    CloseButtonHandler = () => {
        this.modal.modal('hide');
    }

    public render() {
        return <div className="modal fade" id="EditUserModal" role="dialog" data-backdrop={false}>
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <button type="button" className="close" aria-label="Close" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}><span aria-hidden="true">&times;</span></button>
                        <h4 className="modal-title" id="EditUserLabel">Edit user password</h4>
                    </div>
                    <div className="modal-body">
                        <input type="password" className="modalInputPassword" placeholder="Enter Password" ref="inputPW" />
                        <p className="validateModalInputPassword" ref="input_pw_error"></p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-default" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}>Close</button>
                        <button type="button" className="btn btn-primary" onClick={() => {
                            this.EditHandler() ? this.props.CloseClickHandler() : null
                        }}>Edit</button>
                    </div>
                </div>
            </div>
        </div>
    }
}
