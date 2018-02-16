import * as React from 'react';
interface InputProps {
    onClickHandler: () => void,
}
export default class EditUserModalButton extends React.Component<InputProps, {}> {
    public render() {
        return <button type="button" className="btn btn-default" onClick={this.props.onClickHandler}>
            <span className="glyphicon glyphicon-pencil" aria-hidden="true"></span>
        </button>
    }
}
