import * as React from 'react';
interface InputProps {
    IsCanAuthohorization?:boolean,
    onClickHandler: () => void,
}
export default class DeleteWarningModalButton extends React.Component<InputProps, {}> {
    public render() {
        return <button type="button" className="btn btn-default" onClick={this.props.onClickHandler}>
            {
                this.props.IsCanAuthohorization 
                    ? <span className="glyphicon glyphicon-remove" aria-hidden="true"></span>
                    : <span className="glyphicon glyphicon-plus" aria-hidden="true"></span>
            }
        </button>
    }
}
