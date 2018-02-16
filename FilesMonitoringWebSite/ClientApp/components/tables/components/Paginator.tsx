import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import * as NavMenu from '../components/NavMenu';

type InputProps = {
    currentPage: number,
    CountView: number,
    movePageAction: any,
}

export default class Paginator extends React.Component<InputProps, {}> {
    public render() {
        return <div className="btn-group btn-group-sm" role="group">
            <button type="button" className="btn btn-default" onClick={() => this.props.movePageAction(-1)}><span className="glyphicon glyphicon-chevron-left" aria-hidden="true"></span></button>
            <button type="button" className="btn btn-default" onClick={() => this.props.movePageAction(1)}><span className="glyphicon glyphicon-chevron-right" aria-hidden="true"></span></button>
        </div>;
    }
}