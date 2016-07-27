﻿// A '.tsx' file enables JSX support in the TypeScript compiler, 
// for more information see the following page on the TypeScript wiki:
// https://github.com/Microsoft/TypeScript/wiki/JSX
import * as React from "react";

import { Router, Route, IndexRoute, hashHistory, browserHistory } from "react-router";
import { Main } from "../components/Main";
import { UserManagementContainer } from "../containers/UserManagementContainer";
import { Portal } from "../components/Portal";

export var routes = (
    <Router history={ browserHistory }>
        <Route path="/manage" component={ Main }>
            <IndexRoute component={ Portal }/>
            <Route path="/manage/users" component={ UserManagementContainer } />
        </Route>
    </Router>
);

