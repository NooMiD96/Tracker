import * as React from 'react';
interface InputProps {
    onClickHandler:any,
}
export default class EditDeleteTimeModalButton extends React.Component<InputProps, {}> {
    public render() {
        return <button type="button" className={"btn btn-default right"} onClick={this.props.onClickHandler}>
            <span className="glyphicon glyphicon-pencil" aria-hidden="true"></span>
        </button>
    }
}
