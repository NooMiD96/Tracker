import * as React from 'react';
import { NavLink, Link } from 'react-router-dom';
import { connect } from 'react-redux';
import LoginModal from './LoginModal';
import * as UserState from '../store/User';

export type UserProps =
    UserState.UserState
    & typeof UserState.actionCreators;
    //& RouteComponentProps<{}>;          // ... plus incoming routing parameters   


export class NavMenu extends React.Component<UserProps, {}> {
    ToLogin = <ul className='nav navbar-nav navbar-right'>
        <li>
            <button type="button" className="btn btn-default btn-lg navbar-left" data-toggle="modal" data-target="#loginModal">
                Login
            </button>
            <LoginModal TrySignInFetch={UserState.functions.TrySignInFetch} GetUserInfo={this.props.GetUserInfo}/>
        </li>
    </ul>

    ToLogout = (userName: string | null, userType: string) => <ul className='nav navbar-nav navbar-right'>
        <li><span className="label label-default">{userName}</span></li>
        <li>
            {
                userType == "Admin" 
                    ? <button type="button" className="btn btn-default btn-lg" onClick={this.props.AdminTrigger}>
                        Administrating
                    </button>
                    : null
            }
            <button type="button" className="btn btn-default btn-lg" onClick={this.props.SignOut}>
                Logout
            </button>
        </li>
    </ul>

    componentDidMount() {
        this.props.GetUserInfo();
    }

    public render() {
        return <div className='main-nav'>
            <div className='navbar navbar-inverse '>
                <div className="container-fluid">
                    <div className='navbar-header'>
                        <button type='button' className='navbar-toggle collapsed' data-toggle='collapse' data-target='#menu-navbar'>
                            <span className='sr-only'>Toggle navigation</span>
                            <span className='icon-bar'></span>
                            <span className='icon-bar'></span>
                            <span className='icon-bar'></span>
                        </button>
                        <Link className='navbar-brand' to={'/'}>Tracker</Link>
                    </div>
                    {/* <div className='clearfix'></div> */}
                    <div className='navbar-link navbar-collapse collapse' id='menu-navbar'>
                        <ul className='nav navbar-nav'>
                            <li>
                                <NavLink exact to={'/'} activeClassName='' className="NavLinks">
                                    <span className='glyphicon glyphicon-home'></span> Home
                                </NavLink>
                                {
                                    this.props.userType == "Admin" 
                                    ? <NavLink exact to={'/Exceptions'} activeClassName='' className="NavLinks">
                                        <span className='glyphicon glyphicon-exclamation-sign'></span> Exceptions
                                    </NavLink>
                                    : null
                                }
                            </li>
                        </ul>
                        {
                            this.props.userType == null
                            ? this.ToLogin 
                            : this.ToLogout(this.props.userName, this.props.userType)
                        }
                    </div>
                </div>
            </div>
        </div>;
    }
}


function mapStateToProps(state: any) {
    return {
        ...state.user
    } as UserState.UserState;
}

const mapDispatchToProps = {
    ...UserState.actionCreators,
};

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(NavMenu) as typeof NavMenu;
