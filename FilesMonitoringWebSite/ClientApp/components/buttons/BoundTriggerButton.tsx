import * as React from 'react';

interface InputProps {
    userType?: string,
    isBoundUserName: boolean,
    BoundTrigger: () => void
}

export default class BoundTriggerButton extends React.Component<InputProps, {}> {
    public render() {
        return this.props.userType == "Admin" &&
            <div className="col-md-4">
                <button type="button" className="btn btn-default btn-lg btn-bound-trigger" onClick={this.props.BoundTrigger}>
                    {
                        this.props.isBoundUserName
                            ? "Unbound user name to tracker"
                            : "Bound user name to tracker"
                    }
                </button> 
            </div>
    }
}
