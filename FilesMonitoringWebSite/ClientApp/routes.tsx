import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './containers/Layout';
import UserChanges from './containers/UserChanges';
import UserExceptions from './containers/UserExceptions';

export const routes = <Layout>
    <Route exact path='/' component={ UserChanges } />
    <Route exact path='/Exceptions' component={ UserExceptions } />
</Layout>;
