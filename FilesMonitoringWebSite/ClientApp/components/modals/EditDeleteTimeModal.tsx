import * as React from 'react';
import * as ReactDOM from 'react-dom';
import * as $ from 'jquery';
import * as DatePicker from 'react-datetime';

interface InputProps {
    EditDeleteTime: (date: Date, isNeedDelete:boolean) => void
    CloseClickHandler: () => void,
    FileId:number,
    isNeedDelete: boolean,
}

export default class EditDeleteTimeModal extends React.Component<InputProps, {}> {
    date: any;
    modal: any;

    componentDidMount(){
        this.modal = $('#EditDeleteTimeModal');
        this.modal.modal('show');
        this.date = new Date();
    }

    validDate(current: Date) : boolean {
        var date = new Date();
        date.setDate(date.getDate() - 1);
        
        if(current < date) {
            return false
        }
        return true;
    }
    EditHandler = () => {
        var DOM_DataPicker = ReactDOM.findDOMNode(this.refs.DatePicker) as HTMLDivElement;
        var DOM_input = DOM_DataPicker.firstChild as HTMLInputElement;
        var DOM_radio = ReactDOM.findDOMNode(this.refs.needDeleteRadio) as HTMLInputElement;

        var date;
        try{
            date = new Date(this.date.toDate());
        } catch(e) {
            date = this.date;
        }
        
        date.setUTCHours(0,0,0,0);

        this.props.EditDeleteTime(date, DOM_radio.checked);

        this.modal.modal('hide');
        return false;
    }
    CloseButtonHandler = () => {
        this.modal.modal('hide');
    }

    public render() {
        return <div className="modal fade" id="EditDeleteTimeModal" role="dialog" data-backdrop={false}>
            <div className="modal-dialog" role="document">
                <div className="modal-content">
                    <div className="modal-header">
                        <button type="button" className="close" aria-label="Close" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}><span aria-hidden="true">&times;</span></button>
                        <h4 className="modal-title" id="EditDeleteTimeLabel">Edit delete time</h4>
                    </div>
                    <div className="modal-body">
                        <input type="checkbox" id={"checkbox" + this.props.FileId} ref="needDeleteRadio" value="needDelete" defaultChecked={true}/>
                        <label htmlFor={"checkbox" + this.props.FileId} style={{fontWeight: "normal"}}>Need delete file changes?</label>
                        <DatePicker defaultValue={new Date()} isValidDate={this.validDate} timeFormat={false} ref="DatePicker"
                            locale={window.navigator.language} onChange={(e) => this.date = e}/>
                        <p className="validateModalInputDate" ref="input_date_error"></p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-default" onClick={() => { 
                            this.CloseButtonHandler(); this.props.CloseClickHandler();
                        }}>Close</button>
                        <button type="button" className="btn btn-primary" onClick={() => {
                            this.EditHandler(); this.props.CloseClickHandler();
                        }}>Add</button>
                    </div>
                </div>
            </div>
        </div>
    }
}
