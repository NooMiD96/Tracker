import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';

interface InputProps {
    SaveFilter: any,
    ResetFilter: any,
    ResetChangeList: any,
}

export default class SearchField extends React.Component<InputProps, {}> {
    input: HTMLInputElement | null;

    InputHandler = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if(event.key == "Enter"){
            this.SetFilter();
        }
    }
    SetFilter = () => {
        if(!this.input){
            this.props.ResetFilter();
            this.props.ResetChangeList();
            return;
        }
        var inputValue = this.input.value.trim();
        if(inputValue == ''){
            this.props.ResetFilter();
            this.props.ResetChangeList();
            return;
        }
        this.props.SaveFilter(inputValue);
        this.props.ResetChangeList();
    }
    
    public render() {
        return <div className="col-md-4">
            <div className="input-group">
                <input type="text" className="form-control search-input" placeholder="Input name or path of file" onKeyPress={this.InputHandler.bind(this)} ref={inputRef => this.input = inputRef}/>
                <div className="input-group-addon input-addon-clickable" onClick={this.SetFilter.bind(this)}><span className="glyphicon glyphicon-glass" aria-hidden="true"></span></div>
            </div>
        </div>;
    }
}
