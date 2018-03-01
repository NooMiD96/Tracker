import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';
import NavMenu from '../NavMenu';
import * as $ from 'jquery';

interface InputProps {
    TrySignInFetch: (userName: string, userPass: string) => Promise<boolean | void>,
    GetUserInfo: () => any,
}

export default class LoginModal extends React.Component<InputProps, {}> {
    loginhandler = () => {
        var isEmpty = false;
        var DOM_inputUN = ReactDOM.findDOMNode(this.refs.inputUN) as HTMLInputElement,
            DOM_inputPW = ReactDOM.findDOMNode(this.refs.inputPW) as HTMLInputElement,
            DOM_incorrect_login = ReactDOM.findDOMNode(this.refs.incorrect_login) as HTMLElement;

        if (DOM_inputUN.value == "") {
            DOM_inputUN.style.setProperty("border-color", "red");
            isEmpty = true;
        } else {
            DOM_inputUN.setAttribute("style", "border-color: initial;");
        }
        if (DOM_inputPW.value == "") {
            DOM_inputPW.style.setProperty("border-color", "red");
            isEmpty = true;
        } else {
            DOM_inputPW.setAttribute("style", "border-color: initial;");
        }
        if (!isEmpty) {
            this.props.TrySignInFetch(DOM_inputUN.value, DOM_inputPW.value)
                .then(data => {
                    if(data) {
                        ($('#loginModal') as any).modal('toggle');
                        this.props.GetUserInfo();
                    } else {
                        DOM_incorrect_login.innerHTML = "Incorrect user name or password";
                    }
                });

            DOM_inputPW.value = "";
        }
    }
    
    public render() {
        return <div className="modal fade" id="loginModal" role="dialog" aria-labelledby="loginModalLabel" data-backdrop={false}>
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <button type="button" className="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                        <h4 className="modal-title" id="loginModalLabel">Login</h4>
                    </div>
                    <div className="modal-body">
                        <p className="incorrectLogin" ref="incorrect_login"></p>
                        <input type="text" className="modalInputUserName" placeholder="Enter your User Name" ref="inputUN" />
                        <p className="validateModalInputUserName" ref="input_un_error"></p>
                        <input type="password" className="modalInputPassword" placeholder="Enter your Password" ref="inputPW" onKeyPress={(ev) => {
                            if(ev.key == 'Enter')
                                this.loginhandler()
                        }} />
                        <p className="validateModalInputPassword" ref="input_pw_error"></p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-default" data-dismiss="modal">Close</button>
                        <button type="button" className="btn btn-primary" onClick={this.loginhandler}>Login</button>
                    </div>
                </div>
            </div>
        </div>
    }
}
