import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import * as NavMenu from '../components/NavMenu';

type InputProps = {
    ViewCounterAction: any,
    ViewNow: number,
}

export default class Paginator extends React.Component<InputProps, {}> {
    public render() {
        return <div className="btn-group btn-group-sm right" role="group">
            <button type="button" className={"btn btn-default" + (this.props.ViewNow == 10 ? " disabled default-cursor" : "")} 
                onClick={() => this.props.ViewCounterAction(10)}>10
            </button>
            <button type="button" className={"btn btn-default" + (this.props.ViewNow == 25 ? " disabled default-cursor" : "")} 
                onClick={() => this.props.ViewCounterAction(25)}>25
            </button>
            <button type="button" className={"btn btn-default" + (this.props.ViewNow == 50 ? " disabled default-cursor" : "")} 
                onClick={() => this.props.ViewCounterAction(50)}>50
            </button>
            <button type="button" className={"btn btn-default" + (this.props.ViewNow == 100 ? " disabled default-cursor" : "")} 
                onClick={() => this.props.ViewCounterAction(100)}>100
            </button>
        </div>;
    }
}