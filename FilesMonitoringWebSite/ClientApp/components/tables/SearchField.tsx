import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';
import NavMenu from './NavMenu';

interface InputProps {
    SetFilter: any,
    DeleteFilter: any,
    ResetChangeList: any,
}

export default class SearchField extends React.Component<InputProps, {}> {
    InputHandler = (event: React.KeyboardEvent<HTMLInputElement>) => {
        if(event.key == "Enter"){
            var inputValue = event.currentTarget.value.trim();
            if(inputValue == ''){
                this.props.DeleteFilter();
                this.props.ResetChangeList();
                return;
            }
            this.props.SetFilter(inputValue);
            this.props.ResetChangeList();
        }
    }
    public render() {
        return <input type="text" className="search-input" placeholder="Input name or path of file" onKeyPress={this.InputHandler}/>;
    }
}
