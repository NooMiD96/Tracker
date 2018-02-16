import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import * as NavMenu from '../components/NavMenu';

export class Layout extends React.Component<{}, {}> {
    public render() {
        return <div className='container-fluid'>
            {React.createElement(NavMenu.default, {} as NavMenu.UserProps)}
            {this.props.children}
            <div id="modal-container"></div>
        </div>;
    }
}