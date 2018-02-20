import * as React from 'react';
interface InputProps {
    printText: string,
    onClickAction: () => void
}
export default class EditDeleteTimeModalButton extends React.Component<InputProps, {}> {
    public render() {
        return <div style={{margin: '10px 0px 10px 0px'}}>
            <p style={{display: 'inline-block', marginRight: '10px'}}>{this.props.printText}</p>
            <button type="button" className={"btn btn-default"} 
                onClick={this.props.onClickAction}><span className="glyphicon glyphicon-remove" aria-hidden="true"></span>
            </button>
        </div>
    }
}
